
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GeneradorMeteoritosTests
{
    private GameObject generadorObject;
    private GameObject camaraObject;
    private GameObject jugadorObject;
    private GameObject poolObject;
    private GameObject prefabObject;

    private GeneradorMeteoritos generador;
    private MeteoritoPool pool;
    private Meteorito prefab;
    private Camera camara;

    [SetUp]
    public void SetUp()
    {
        // Crear camara
        camaraObject = new GameObject("Camera_Test");
        camara = camaraObject.AddComponent<Camera>();
        camara.orthographic = true;
        camara.orthographicSize = 5f;
        camara.aspect = 2f;

        // Crear jugador
        jugadorObject = new GameObject("Player_Test");
        jugadorObject.transform.position = new Vector3(100f, 0f, 0f);

        // Crear prefab
        prefabObject = new GameObject("Meteorito_Prefab_Test");
        prefabObject.AddComponent<Rigidbody2D>();
        prefab = prefabObject.AddComponent<Meteorito>();
        prefabObject.SetActive(false);

        // Crear pool sin activar
        poolObject = new GameObject("Pool_Test");
        poolObject.SetActive(false);

        pool = poolObject.AddComponent<MeteoritoPool>();

        SetPrivateField(
            pool,
            "prefabMeteorito",
            prefab
        );

        SetPrivateField(pool, "cantidadInicial", 1);
        SetPrivateField(pool, "maximoMeteoritos", 3);

        LogAssert.Expect(
            LogType.Log,
            "MeteoritoPool inicializado. Meteoritos creados: 1"
        );

        poolObject.SetActive(true);

        // Crear generador
        generadorObject = new GameObject("Generador_Test");
        generador = generadorObject.AddComponent<GeneradorMeteoritos>();

        SetPrivateField(generador, "pool", pool);
        SetPrivateField(generador, "jugador", jugadorObject.transform);
        SetPrivateField(generador, "camara", camara);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(generadorObject);
        Object.DestroyImmediate(poolObject);
        Object.DestroyImmediate(prefabObject);
        Object.DestroyImmediate(camaraObject);
        Object.DestroyImmediate(jugadorObject);
    }

    private void SetPrivateField(
        object target,
        string fieldName,
        object value)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(
            field,
            "No se ha encontrado el campo " + fieldName
        );

        field.SetValue(target, value);
    }

    private object InvokePrivateMethod(string methodName)
    {
        MethodInfo method = typeof(GeneradorMeteoritos).GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(
            method,
            "No se ha encontrado el metodo " + methodName
        );

        return method.Invoke(generador, null);
    }

    [Test]
    public void IniciarOleada_StartsSuccessfully()
    {
        LogAssert.Expect(
            LogType.Log,
            "GeneradorMeteoritos: Comienza la lluvia de meteoritos."
        );

        LogAssert.Expect(
            LogType.Log,
            "Meteoritos generados durante la oleada: 1"
        );

        bool resultado = generador.IniciarOleada(
            2f,
            8f,
            1
        );

        Assert.IsTrue(resultado);

        LogAssert.Expect(
            LogType.Log,
            "GeneradorMeteoritos: Oleada detenida."
        );

        generador.DetenerOleada();
    }

    [Test]
    public void IniciarOleada_RejectsSecondActiveWave()
    {
        LogAssert.Expect(
            LogType.Log,
            "GeneradorMeteoritos: Comienza la lluvia de meteoritos."
        );

        LogAssert.Expect(
            LogType.Log,
            "Meteoritos generados durante la oleada: 1"
        );

        Assert.IsTrue(
            generador.IniciarOleada(2f, 8f, 1)
        );

        LogAssert.Expect(
            LogType.Warning,
            "GeneradorMeteoritos: Ya hay una oleada en curso."
        );

        Assert.IsFalse(
            generador.IniciarOleada(2f, 8f, 1)
        );

        LogAssert.Expect(
            LogType.Log,
            "GeneradorMeteoritos: Oleada detenida."
        );

        generador.DetenerOleada();
    }

    [Test]
    public void IniciarOleada_RejectsMissingReferences()
    {
        SetPrivateField(generador, "pool", null);

        LogAssert.Expect(
            LogType.Error,
            "GeneradorMeteoritos: Faltan referencias en el Inspector."
        );

        bool resultado = generador.IniciarOleada(
            2f,
            8f,
            1
        );

        Assert.IsFalse(resultado);
    }

    [Test]
    public void IniciarOleada_ClampsSpeedAndDamage()
    {
        LogAssert.Expect(
            LogType.Log,
            "GeneradorMeteoritos: Comienza la lluvia de meteoritos."
        );

        LogAssert.Expect(
            LogType.Log,
            "Meteoritos generados durante la oleada: 1"
        );

        Assert.IsTrue(
            generador.IniciarOleada(0f, -5f, 0)
        );

        // Obtener el meteorito creado
        Meteorito meteorito = poolObject.GetComponentInChildren<Meteorito>();

        Assert.IsNotNull(meteorito);

        Rigidbody2D rb = meteorito.GetComponent<Rigidbody2D>();

        Assert.AreEqual(
            -0.1f,
            rb.linearVelocity.y,
            0.001f
        );

        // Comprobar que el dano minimo es 1.
        GameObject jugador = new GameObject("Player_Damage_Test");
        jugador.tag = "Player";
        jugador.AddComponent<BoxCollider2D>();

        PlayerHealth health = jugador.AddComponent<PlayerHealth>();

        MethodInfo trigger = typeof(Meteorito).GetMethod(
            "OnTriggerEnter2D",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(trigger);

        trigger.Invoke(
            meteorito,
            new object[] { jugador.GetComponent<Collider2D>() }
        );

        Assert.AreEqual(2, health.CurrentHealth);

        Object.DestroyImmediate(jugador);

        LogAssert.Expect(
            LogType.Log,
            "GeneradorMeteoritos: Oleada detenida."
        );

        generador.DetenerOleada();
    }

    [Test]
    public void ObtenerPosicionAparicion_IsAboveCamera()
    {
        camaraObject.transform.position = new Vector3(3f, 4f, 0f);

        Vector3 posicion = (Vector3)InvokePrivateMethod(
            "ObtenerPosicionAparicion"
        );

        Assert.AreEqual(
            11f,
            posicion.y,
            0.001f
        );

        Assert.AreEqual(0f, posicion.z, 0.001f);
    }

    [Test]
    public void ObtenerPosicionAparicion_StaysWithinHorizontalCameraBounds()
    {
        camaraObject.transform.position = new Vector3(3f, 4f, 0f);

        Vector3 posicion = (Vector3)InvokePrivateMethod(
            "ObtenerPosicionAparicion"
        );

        Assert.GreaterOrEqual(posicion.x, -6.5f);
        Assert.LessOrEqual(posicion.x, 12.5f);
    }

    [Test]
    public void ObtenerPosicionAparicion_KeepsDistanceFromPlayer()
    {
        jugadorObject.transform.position =
            new Vector3(100f, 0f, 0f);

        Vector3 posicion = (Vector3)InvokePrivateMethod(
            "ObtenerPosicionAparicion"
        );

        float distancia = Mathf.Abs(
            posicion.x - jugadorObject.transform.position.x
        );

        Assert.GreaterOrEqual(distancia, 2f);
    }

    [Test]
    public void DetenerOleada_WithoutActiveWaveDoesNothing()
    {
        Assert.DoesNotThrow(
            () => generador.DetenerOleada()
        );
    }
}
