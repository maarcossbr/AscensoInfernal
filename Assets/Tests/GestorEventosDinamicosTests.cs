
using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GestorEventosDinamicosTests
{
    private GameObject gestorObject;
    private GameObject generadorObject;
    private GameObject poolObject;
    private GameObject prefabObject;
    private GameObject jugadorObject;
    private GameObject camaraObject;

    private GestorEventosDinamicos gestor;
    private GeneradorMeteoritos generador;
    private MeteoritoPool pool;
    private Meteorito prefab;
    private Camara camara;

    [SetUp]
    public void SetUp()
    {
        jugadorObject = new GameObject("Player_Test");

        camaraObject = new GameObject("Camara_Test");
        camara = camaraObject.AddComponent<Camara>();

        Camera unityCamera = camaraObject.AddComponent<Camera>();
        unityCamera.orthographic = true;
        unityCamera.orthographicSize = 5f;

        prefabObject = new GameObject("Meteorito_Prefab_Test");
        prefabObject.AddComponent<Rigidbody2D>();
        prefab = prefabObject.AddComponent<Meteorito>();
        prefabObject.SetActive(false);

        poolObject = new GameObject("Pool_Test");
        poolObject.SetActive(false);

        pool = poolObject.AddComponent<MeteoritoPool>();

        SetPrivateField(pool, "prefabMeteorito", prefab);
        SetPrivateField(pool, "cantidadInicial", 1);
        SetPrivateField(pool, "maximoMeteoritos", 10);

        LogAssert.Expect(
            LogType.Log,
            "MeteoritoPool inicializado. Meteoritos creados: 1"
        );

        poolObject.SetActive(true);

        generadorObject = new GameObject("Generador_Test");
        generador = generadorObject.AddComponent<GeneradorMeteoritos>();

        SetPrivateField(generador, "pool", pool);
        SetPrivateField(generador, "jugador", jugadorObject.transform);
        SetPrivateField(generador, "camara", unityCamera);

        gestor = CrearGestor();

        SetPrivateField(gestor, "retrasoMeteoritos", 1f);

        ConfigurarOleadas(
            gestor,
            10f,
            30f,
            60f,
            90f
        );
    }

    [TearDown]
    public void TearDown()
    {
        DetenerCorrutinaGenerador();

        if (gestorObject != null)
            UnityEngine.Object.DestroyImmediate(gestorObject);

        if (generadorObject != null)
            UnityEngine.Object.DestroyImmediate(generadorObject);

        if (poolObject != null)
            UnityEngine.Object.DestroyImmediate(poolObject);

        if (prefabObject != null)
            UnityEngine.Object.DestroyImmediate(prefabObject);

        if (jugadorObject != null)
            UnityEngine.Object.DestroyImmediate(jugadorObject);

        if (camaraObject != null)
            UnityEngine.Object.DestroyImmediate(camaraObject);
    }

    private GestorEventosDinamicos CrearGestor()
    {
        gestorObject = new GameObject("Gestor_Test");

        GestorEventosDinamicos nuevoGestor =
            gestorObject.AddComponent<GestorEventosDinamicos>();

        SetPrivateField(
            nuevoGestor,
            "jugador",
            jugadorObject.transform
        );

        SetPrivateField(
            nuevoGestor,
            "generador",
            generador
        );

        SetPrivateField(
            nuevoGestor,
            "camara",
            camara
        );

        return nuevoGestor;
    }

    private void ConfigurarOleadas(
        GestorEventosDinamicos objetivo,
        params float[] alturas)
    {
        Type tipoConfiguracion =
            typeof(GestorEventosDinamicos).GetNestedType(
                "ConfiguracionOleada",
                BindingFlags.NonPublic
            );

        Assert.IsNotNull(tipoConfiguracion);

        Array configuraciones = Array.CreateInstance(
            tipoConfiguracion,
            alturas.Length
        );

        for (int i = 0; i < alturas.Length; i++)
        {
            object configuracion =
                Activator.CreateInstance(tipoConfiguracion);

            tipoConfiguracion.GetField("alturaActivacion")
                .SetValue(configuracion, alturas[i]);

            tipoConfiguracion.GetField("intervalo")
                .SetValue(configuracion, 0.5f);

            tipoConfiguracion.GetField("velocidad")
                .SetValue(configuracion, 9f);

            tipoConfiguracion.GetField("dano")
                .SetValue(configuracion, 1);

            tipoConfiguracion.GetField("cantidadMeteoritos")
                .SetValue(configuracion, 1);

            configuraciones.SetValue(configuracion, i);
        }

        SetPrivateField(objetivo, "oleadas", configuraciones);
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

    private T GetPrivateField<T>(
        object target,
        string fieldName)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(
            field,
            "No se ha encontrado el campo " + fieldName
        );

        return (T)field.GetValue(target);
    }

    private void InvokeUpdate(GestorEventosDinamicos objetivo)
    {
        MethodInfo method = typeof(GestorEventosDinamicos).GetMethod(
            "Update",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(method);

        method.Invoke(objetivo, null);
    }

    private void ExpectWaveStart(int number)
    {
        LogAssert.Expect(
            LogType.Log,
            "TERREMOTO ACTIVADO"
        );

        LogAssert.Expect(
            LogType.Log,
            "GestorEventosDinamicos: Oleada "
            + number + " iniciada."
        );
    }

    private void SimulateWaveCompletion()
    {
        DetenerCorrutinaGenerador();

        SetPrivateField(
            generador,
            "oleadaEnCurso",
            false
        );

        PropertyInfo property =
            typeof(GeneradorMeteoritos).GetProperty(
                "OleadaCompletada"
            );

        Assert.IsNotNull(property);

        FieldInfo backingField =
            typeof(GeneradorMeteoritos).GetField(
                "<OleadaCompletada>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

        Assert.IsNotNull(backingField);

        backingField.SetValue(generador, true);
    }

    private void DetenerCorrutinaGenerador()
    {
        if (generador == null)
            return;

        Coroutine rutina = GetPrivateField<Coroutine>(
            generador,
            "rutinaOleada"
        );

        if (rutina != null)
            generador.StopCoroutine(rutina);

        SetPrivateField(generador, "rutinaOleada", null);
        SetPrivateField(generador, "oleadaEnCurso", false);
    }

    [Test]
    public void Update_BelowFirstThreshold_DoesNotStartWave()
    {
        jugadorObject.transform.position =
            new Vector3(0f, 5f, 0f);

        InvokeUpdate(gestor);

        Assert.AreEqual(
            -1,
            GetPrivateField<int>(
                gestor,
                "indiceOleadaEnCurso"
            )
        );

        Assert.IsFalse(generador.OleadaEnCurso);
    }

    [Test]
    public void Update_SelectsHighestReachedWave()
    {
        jugadorObject.transform.position =
            new Vector3(0f, 45f, 0f);

        ExpectWaveStart(2);

        InvokeUpdate(gestor);

        Assert.AreEqual(
            1,
            GetPrivateField<int>(
                gestor,
                "indiceOleadaEnCurso"
            )
        );

        Assert.IsTrue(generador.OleadaEnCurso);
    }

    [Test]
    public void Update_DoesNotRepeatCompletedWaveDuringSameLife()
    {
        jugadorObject.transform.position =
            new Vector3(0f, 45f, 0f);

        ExpectWaveStart(2);
        InvokeUpdate(gestor);

        SimulateWaveCompletion();

        LogAssert.Expect(
            LogType.Log,
            "GestorEventosDinamicos: Oleada 2 completada."
        );

        InvokeUpdate(gestor);

        Assert.AreEqual(
            -1,
            GetPrivateField<int>(
                gestor,
                "indiceOleadaEnCurso"
            )
        );

        // Seguimos en el mismo tramo.
        // La oleada no debe repetirse.
        InvokeUpdate(gestor);

        Assert.IsFalse(generador.OleadaEnCurso);
    }

    [Test]
    public void Update_StartsNextWaveAfterCrossingNextThreshold()
    {
        jugadorObject.transform.position =
            new Vector3(0f, 20f, 0f);

        ExpectWaveStart(1);
        InvokeUpdate(gestor);

        SimulateWaveCompletion();

        LogAssert.Expect(
            LogType.Log,
            "GestorEventosDinamicos: Oleada 1 completada."
        );

        InvokeUpdate(gestor);

        jugadorObject.transform.position =
            new Vector3(0f, 45f, 0f);

        ExpectWaveStart(2);
        InvokeUpdate(gestor);

        Assert.AreEqual(
            1,
            GetPrivateField<int>(
                gestor,
                "indiceOleadaEnCurso"
            )
        );
    }

    [Test]
    public void NewManager_ReactivatesWaveAtCheckpointHeight()
    {
        // Simulamos que el jugador estaba en la segunda zona.
        jugadorObject.transform.position =
            new Vector3(0f, 45f, 0f);

        ExpectWaveStart(2);
        InvokeUpdate(gestor);

        SimulateWaveCompletion();

        LogAssert.Expect(
            LogType.Log,
            "GestorEventosDinamicos: Oleada 2 completada."
        );

        InvokeUpdate(gestor);

        Assert.IsFalse(generador.OleadaEnCurso);

        // Simulamos la recarga de la escena.
        UnityEngine.Object.DestroyImmediate(gestorObject);
        gestorObject = null;

        GestorEventosDinamicos nuevoGestor = CrearGestor();

        SetPrivateField(
            nuevoGestor,
            "retrasoMeteoritos",
            1f
        );

        ConfigurarOleadas(
            nuevoGestor,
            10f,
            30f,
            60f,
            90f
        );

        // El jugador reaparece en el mismo checkpoint.
        ExpectWaveStart(2);
        InvokeUpdate(nuevoGestor);

        Assert.AreEqual(
            1,
            GetPrivateField<int>(
                nuevoGestor,
                "indiceOleadaEnCurso"
            )
        );

        Assert.IsTrue(generador.OleadaEnCurso);
    }

    [Test]
    public void Update_AdaptsToDifferentNumberOfWaves()
    {
        ConfigurarOleadas(
            gestor,
            10f,
            20f,
            30f,
            40f,
            50f
        );

        jugadorObject.transform.position =
            new Vector3(0f, 45f, 0f);

        ExpectWaveStart(4);
        InvokeUpdate(gestor);

        Assert.AreEqual(
            3,
            GetPrivateField<int>(
                gestor,
                "indiceOleadaEnCurso"
            )
        );
    }

    [Test]
    public void Update_MissingPlayerOrGenerator_DoesNothing()
    {
        SetPrivateField(gestor, "jugador", null);

        Assert.DoesNotThrow(
            () => InvokeUpdate(gestor)
        );

        SetPrivateField(
            gestor,
            "jugador",
            jugadorObject.transform
        );

        SetPrivateField(
            gestor,
            "generador",
            null
        );

        Assert.DoesNotThrow(
            () => InvokeUpdate(gestor)
        );

        Assert.AreEqual(
            -1,
            GetPrivateField<int>(
                gestor,
                "indiceOleadaEnCurso"
            )
        );
    }
}
