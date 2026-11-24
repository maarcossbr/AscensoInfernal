using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class PlayerMovementTests
{
    private GameObject player;
    private PlayerMovement movement;
    private Rigidbody2D rb;

    [SetUp]
    public void SetUp()
    {
        player = new GameObject("PlayerMovement_Test");

        rb = player.AddComponent<Rigidbody2D>();
        movement = player.AddComponent<PlayerMovement>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(player);
    }

    // Acceso a campos privados para preparar los escenarios.
    private void SetField(string fieldName, object value)
    {
        FieldInfo field = typeof(PlayerMovement).GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(field, "No existe el campo: " + fieldName);

        field.SetValue(movement, value);
    }

    private T GetField<T>(string fieldName)
    {
        FieldInfo field = typeof(PlayerMovement).GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(field, "No existe el campo: " + fieldName);

        return (T)field.GetValue(movement);
    }

    private object InvokeMethod(string methodName)
    {
        MethodInfo method = typeof(PlayerMovement).GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(method, "No existe el metodo: " + methodName);

        return method.Invoke(movement, null);
    }

    [Test]
    public void Awake_ConfiguresRigidbody()
    {
        Assert.AreEqual(3f, rb.gravityScale);
        Assert.IsTrue(rb.freezeRotation);
    }

    [Test]
    public void MoveHorizontally_AcceleratesPlayer()
    {
        SetField("horizontalInput", 1f);

        InvokeMethod("MoveHorizontally");

        Assert.AreEqual(
            0.7f,
            rb.linearVelocity.x,
            0.01f
        );
    }

    [Test]
    public void MoveHorizontally_DeceleratesPlayer()
    {
        rb.linearVelocity = new Vector2(5f, 0f);
        SetField("horizontalInput", 0f);

        InvokeMethod("MoveHorizontally");

        Assert.AreEqual(
            4.1f,
            rb.linearVelocity.x,
            0.01f
        );
    }

    [Test]
    public void PerformJump_AppliesVerticalForce()
    {
        InvokeMethodWithFloat("PerformJump", 12f);

        Assert.AreEqual(12f, rb.linearVelocity.y);
    }

    private void InvokeMethodWithFloat(
        string methodName,
        float value)
    {
        MethodInfo method = typeof(PlayerMovement).GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(method, "No existe el metodo: " + methodName);

        method.Invoke(movement, new object[] { value });
    }

    [Test]
    public void HandleJump_PerformsFirstJump()
    {
        SetField("jumpBufferCounter", 1f);
        SetField("coyoteCounter", 1f);

        InvokeMethod("HandleJump");

        Assert.AreEqual(12f, rb.linearVelocity.y);
        Assert.IsTrue(GetField<bool>("hasUsedFirstJump"));
        Assert.AreEqual(0f, GetField<float>("jumpBufferCounter"));
    }

    [Test]
    public void HandleJump_PerformsDoubleJump()
    {
        SetField("hasUsedFirstJump", true);
        SetField("airJumpsRemaining", 1);
        SetField("jumpBufferCounter", 1f);

        InvokeMethod("HandleJump");

        Assert.AreEqual(11.5f, rb.linearVelocity.y);
        Assert.AreEqual(0, GetField<int>("airJumpsRemaining"));
    }

    [Test]
    public void HandleJump_CutsJumpHeightWhenReleased()
    {
        rb.linearVelocity = new Vector2(0f, 10f);

        SetField("jumpReleased", true);
        SetField("jumpCutApplied", false);

        InvokeMethod("HandleJump");

        Assert.AreEqual(5f, rb.linearVelocity.y);
        Assert.IsTrue(GetField<bool>("jumpCutApplied"));
    }

    [Test]
    public void TryWallJump_JumpsAwayFromLeftWall()
    {
        SetField("isGrounded", false);
        SetField("isTouchingWallLeft", true);
        SetField("isTouchingWallRight", false);

        bool result = (bool)InvokeMethod("TryWallJump");

        Assert.IsTrue(result);
        Assert.AreEqual(8f, rb.linearVelocity.x);
        Assert.AreEqual(12f, rb.linearVelocity.y);
        Assert.IsTrue(GetField<bool>("isWallJumping"));
    }

    [Test]
    public void TryWallJump_JumpsAwayFromRightWall()
    {
        SetField("isGrounded", false);
        SetField("isTouchingWallLeft", false);
        SetField("isTouchingWallRight", true);

        bool result = (bool)InvokeMethod("TryWallJump");

        Assert.IsTrue(result);
        Assert.AreEqual(-8f, rb.linearVelocity.x);
        Assert.AreEqual(12f, rb.linearVelocity.y);
    }

    [Test]
    public void TryWallJump_DoesNotWorkOnGround()
    {
        SetField("isGrounded", true);
        SetField("isTouchingWallLeft", true);

        bool result = (bool)InvokeMethod("TryWallJump");

        Assert.IsFalse(result);
    }

    [Test]
    public void TryStartDash_ActivatesDash()
    {
        InvokeMethod("TryStartDash");

        Assert.IsTrue(GetField<bool>("isDashing"));
        Assert.IsFalse(GetField<bool>("canDash"));

        Assert.AreEqual(0f, rb.gravityScale);
        Assert.AreEqual(10f, rb.linearVelocity.x);
        Assert.AreEqual(0f, rb.linearVelocity.y);
    }

    [Test]
    public void TryStartDash_DoesNotActivateWhenUnavailable()
    {
        SetField("canDash", false);

        InvokeMethod("TryStartDash");

        Assert.IsFalse(GetField<bool>("isDashing"));
        Assert.AreEqual(3f, rb.gravityScale);
    }

    [Test]
    public void StopDash_RestoresGravity()
    {
        SetField("isDashing", true);
        rb.gravityScale = 0f;

        InvokeMethod("StopDash");

        Assert.IsFalse(GetField<bool>("isDashing"));
        Assert.AreEqual(3f, rb.gravityScale);
        Assert.AreEqual(0.15f, GetField<float>("dashCooldownTimer"));
    }

    [Test]
    public void AdjustGravity_IncreasesGravityWhenFalling()
    {
        rb.linearVelocity = new Vector2(0f, -5f);

        InvokeMethod("AdjustGravity");

        Assert.AreEqual(4.5f, rb.gravityScale);
    }

    [Test]
    public void AdjustGravity_UsesNormalGravityWhenRising()
    {
        rb.linearVelocity = new Vector2(0f, 5f);

        InvokeMethod("AdjustGravity");

        Assert.AreEqual(3f, rb.gravityScale);
    }
}
