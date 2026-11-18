
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using TMPro;
using UnityEngine.TestTools;

public class GestorMineralesTests
{
    private GameObject gestorObject;
    private GameObject textoObject;

    private GestorMinerales gestor;
    private TextMeshPro textoContador;

    [SetUp]
    public void SetUp()
    {
        // Crear el objeto gestor
        gestorObject = new GameObject("GestorMinerales_Test");

        gestor = gestorObject.AddComponent<GestorMinerales>();

        // Crear el texto de la interfaz
        textoObject = new GameObject("TextoContador_Test");

        textoContador = textoObject.AddComponent<TextMeshPro>();

        // Configurar el numero de minerales necesarios
        ConfigurarCampo("mineralesNecesarios", 3);

        // Asignar el texto al gestor
        ConfigurarCampo("textoContador", textoContador);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gestorObject);
        Object.DestroyImmediate(textoObject);
    }

    private void ConfigurarCampo(string nombre, object valor)
    {
        FieldInfo field = typeof(GestorMinerales).GetField(
            nombre,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(
            field,
            "No se ha encontrado el campo " + nombre
        );

        field.SetValue(gestor, valor);
    }

    private void InvocarStart()
    {
        MethodInfo method = typeof(GestorMinerales).GetMethod(
            "Start",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(method);

        method.Invoke(gestor, null);
    }

    [Test]
    public void InitialState_HasZeroMinerals()
    {
        Assert.AreEqual(3, gestor.MineralesRestantes);
        Assert.IsFalse(gestor.TieneTodos);
    }

    [Test]
    public void InitialState_ShowsZeroMineralsInCounter()
    {
        InvocarStart();

        Assert.AreEqual(
            "Minerales: 0/3",
            textoContador.text
        );
    }

    [Test]
    public void RecogerMineral_IncreasesCollectedMinerals()
    {
        LogAssert.Expect(
            UnityEngine.LogType.Log,
            "Minerales recogidos: 1"
        );

        gestor.RecogerMineral();

        Assert.AreEqual(2, gestor.MineralesRestantes);
        Assert.IsFalse(gestor.TieneTodos);
    }

    [Test]
    public void RecogerMineral_UpdatesCounter()
    {
        LogAssert.Expect(
            UnityEngine.LogType.Log,
            "Minerales recogidos: 1"
        );

        gestor.RecogerMineral();

        Assert.AreEqual(
            "Minerales: 1/3",
            textoContador.text
        );
    }

    [Test]
    public void RecogerMineral_RequiredAmount_SetsTieneTodos()
    {
        LogAssert.Expect(
            UnityEngine.LogType.Log,
            "Minerales recogidos: 1"
        );

        LogAssert.Expect(
            UnityEngine.LogType.Log,
            "Minerales recogidos: 2"
        );

        LogAssert.Expect(
            UnityEngine.LogType.Log,
            "Minerales recogidos: 3"
        );

        gestor.RecogerMineral();
        gestor.RecogerMineral();
        gestor.RecogerMineral();

        Assert.IsTrue(gestor.TieneTodos);
        Assert.AreEqual(0, gestor.MineralesRestantes);
    }

    [Test]
    public void RecogerMineral_AfterRequiredAmount_DoesNotIncreaseCounter()
    {
        LogAssert.Expect(
            UnityEngine.LogType.Log,
            "Minerales recogidos: 1"
        );

        LogAssert.Expect(
            UnityEngine.LogType.Log,
            "Minerales recogidos: 2"
        );

        LogAssert.Expect(
            UnityEngine.LogType.Log,
            "Minerales recogidos: 3"
        );

        gestor.RecogerMineral();
        gestor.RecogerMineral();
        gestor.RecogerMineral();

        gestor.RecogerMineral();

        Assert.AreEqual(0, gestor.MineralesRestantes);

        Assert.AreEqual(
            "Minerales: 3/3",
            textoContador.text
        );
    }

    [Test]
    public void RecogerMineral_WithoutText_DoesNotThrowException()
    {
        ConfigurarCampo("textoContador", null);

        LogAssert.Expect(
            UnityEngine.LogType.Log,
            "Minerales recogidos: 1"
        );

        Assert.DoesNotThrow(() =>
        {
            gestor.RecogerMineral();
        });

        Assert.AreEqual(2, gestor.MineralesRestantes);
    }
}
