
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CamaraTests
{
    private GameObject camaraObject;
    private GameObject objetivoObject;

    private Camara camara;
    private Transform objetivo;

    [SetUp]
    public void SetUp()
    {
        camaraObject = new GameObject("Camara_Test");
        camaraObject.transform.position = new Vector3(5f, 10f, -7f);

        camara = camaraObject.AddComponent<Camara>();

        objetivoObject = new GameObject("Objetivo_Test");
        objetivo = objetivoObject.transform;

        ConfigurarCampo("objetivo", objetivo);
        ConfigurarCampo("offsetVertical", 2f);
        ConfigurarCampo("suavizado", 0f);
        ConfigurarCampo("maxAltura", 98f);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(objetivoObject);
        Object.DestroyImmediate(camaraObject);
    }

    private void ConfigurarCampo(string nombre, object valor)
    {
        FieldInfo field = typeof(Camara).GetField(
            nombre,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(field, "No se ha encontrado el campo " + nombre);

        field.SetValue(camara, valor);
    }

    private T ObtenerCampo<T>(string nombre)
    {
        FieldInfo field = typeof(Camara).GetField(
            nombre,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(field, "No se ha encontrado el campo " + nombre);

        return (T)field.GetValue(camara);
    }

    private void InvocarMetodo(string nombre)
    {
        MethodInfo method = typeof(Camara).GetMethod(
            nombre,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(method, "No se ha encontrado el metodo " + nombre);

        method.Invoke(camara, null);
    }

    [Test]
    public void Awake_GuardaPosicionInicialDeLaCamara()
    {
        Assert.AreEqual(10f, ObtenerCampo<float>("alturaBase"));
        Assert.AreEqual(5f, ObtenerCampo<float>("posicionXBase"));
        Assert.AreEqual(-7f, ObtenerCampo<float>("posicionZBase"));
    }

    [Test]
    public void ActualizarSeguimiento_SinObjetivo_NoModificaAltura()
    {
        ConfigurarCampo("objetivo", null);
        ConfigurarCampo("alturaBase", 25f);

        InvocarMetodo("ActualizarSeguimiento");

        Assert.AreEqual(25f, ObtenerCampo<float>("alturaBase"));
    }

    [Test]
    public void ActualizarSeguimiento_ObjetivoEnAlturaPermitida()
    {
        objetivo.position = new Vector3(0f, 12f, 0f);

        ConfigurarCampo("alturaBase", 14f);

        InvocarMetodo("ActualizarSeguimiento");

        Assert.AreEqual(14f, ObtenerCampo<float>("alturaBase"));
    }

    [Test]
    public void ActualizarSeguimiento_ObjetivoSuperaAlturaMaxima()
    {
        objetivo.position = new Vector3(0f, 150f, 0f);

        ConfigurarCampo("alturaBase", 100f);

        InvocarMetodo("ActualizarSeguimiento");

        Assert.AreEqual(100f, ObtenerCampo<float>("alturaBase"));
    }

    [Test]
    public void IniciarSacudida_ValoresPositivos_ActivaSacudida()
    {
        LogAssert.Expect(
            LogType.Log,
            "TERREMOTO ACTIVADO"
        );

        camara.IniciarSacudida(0.8f, 2f);

        Assert.AreEqual(
            0.8f,
            ObtenerCampo<float>("intensidadActual")
        );

        Assert.AreEqual(
            2f,
            ObtenerCampo<float>("tiempoRestanteSacudida")
        );

        Assert.AreEqual(
            2f,
            ObtenerCampo<float>("duracionSacudida")
        );
    }

    [Test]
    public void IniciarSacudida_IntensidadCero_NoActivaSacudida()
    {
        LogAssert.Expect(
            LogType.Log,
            "TERREMOTO ACTIVADO"
        );

        camara.IniciarSacudida(0f, 2f);

        Assert.AreEqual(
            0f,
            ObtenerCampo<float>("tiempoRestanteSacudida")
        );
    }

    [Test]
    public void IniciarSacudida_DuracionCero_NoActivaSacudida()
    {
        LogAssert.Expect(
            LogType.Log,
            "TERREMOTO ACTIVADO"
        );

        camara.IniciarSacudida(0.8f, 0f);

        Assert.AreEqual(
            0f,
            ObtenerCampo<float>("tiempoRestanteSacudida")
        );
    }

    [Test]
    public void IniciarSacudida_ValoresNegativos_NoActivaSacudida()
    {
        LogAssert.Expect(
            LogType.Log,
            "TERREMOTO ACTIVADO"
        );

        camara.IniciarSacudida(-1f, -2f);

        Assert.AreEqual(
            0f,
            ObtenerCampo<float>("tiempoRestanteSacudida")
        );
    }
}
