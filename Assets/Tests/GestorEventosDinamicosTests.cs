
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
        LogAssert.Expect(
            LogType.Log,
            "GestorEventosDinamicos: Progreso de oleadas restablecido."
        );

        GestorEventosDinamicos.ReiniciarProgreso();

        // Crear jugador
        jugadorObject = new GameObject("Player_Test");

        // Crear camara
        camaraObject = new GameObject("Camara_Test");
        camara = camaraObject.AddComponent<Camara>();

        Camera unityCamera = camaraObject.AddComponent<Camera>();
        unityCamera.orthographic = true;
        unityCamera.orthographicSize = 5f;

        // Crear prefab
        prefabObject = new GameObject("Meteorito_Prefab_Test");
        prefabObject.AddComponent<Rigidbody2D>();
        prefab = prefabObject.AddComponent<Meteorito>();
        prefabObject.SetActive(false);

        // Crear pool
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

        // Crear generador
        generadorObject = new GameObject("Generador_Test");
        generador = generadorObject.AddComponent<GeneradorMeteoritos>();

        SetPrivateField(generador, "pool", pool);
        SetPrivateField(generador, "jugador", jugadorObject.transform);
        SetPrivateField(generador, "camara", unityCamera);

        // Crear gestor
        gestorObject = new GameObject("Gestor_Test");
        gestor = gestorObject.AddComponent<GestorEventosDinamicos>();

        SetPrivateField(gestor, "jugador", jugadorObject.transform);
        SetPrivateField(gestor, "generador", generador);
        SetPrivateField(gestor, "camara", camara);

        // Evitar que las corrutinas generen meteoritos durante estas pruebas.
        SetPrivateField(gestor, "retrasoMeteoritos", 1f);
    }

    [TearDown]
    public void TearDown()
    {
        // Evitar logs de detencion durante la destruccion.
        if (generador != null)
        {
            SetPrivateField(generador, "oleadaEnCurso", false);
        }

        Object.DestroyImmediate(gestorObject);
        Object.DestroyImmediate(generadorObject);
        Object.DestroyImmediate(poolObject);
        Object.DestroyImmediate(prefabObject);
        Object.DestroyImmediate(jugadorObject);
        Object.DestroyImmediate(camaraObject);
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

    private void InvokeUpdate()
    {
        MethodInfo method = typeof(GestorEventosDinamicos).GetMethod(
            "Update",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(method);

        method.Invoke(gestor, null);
    }

    private bool IsWaveCompleted(int index)
    {
        MethodInfo method = typeof(GestorEventosDinamicos).GetMethod(
            "EstaCompletada",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(method);

        return (bool)method.Invoke(
            gestor,
            new object[] { index }
        );
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
        SetPrivateField(generador, "oleadaEnCurso", false);

        PropertyInfo property = typeof(GeneradorMeteoritos).GetProperty(
            "OleadaCompletada"
        );

        Assert.IsNotNull(property);

        FieldInfo backingField = typeof(GeneradorMeteoritos).GetField(
            "<OleadaCompletada>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(backingField);

        backingField.SetValue(generador, true);
    }

    [Test]
    public void Update_BelowFirstThreshold_DoesNotStartWave()
    {
        jugadorObject.transform.position =
            new Vector3(0f, 29f, 0f);

        InvokeUpdate();

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
    public void Update_StartsFirstWaveAtRequiredHeight()
    {
        jugadorObject.transform.position =
            new Vector3(0f, 30f, 0f);

        ExpectWaveStart(1);

        InvokeUpdate();

        Assert.AreEqual(
            0,
            GetPrivateField<int>(
                gestor,
                "indiceOleadaEnCurso"
            )
        );

        Assert.IsTrue(generador.OleadaEnCurso);
    }

    [Test]
    public void Update_DoesNotStartAnotherWaveWhileOneIsActive()
    {
        jugadorObject.transform.position =
            new Vector3(0f, 30f, 0f);

        ExpectWaveStart(1);
        InvokeUpdate();

        jugadorObject.transform.position =
            new Vector3(0f, 60f, 0f);

        InvokeUpdate();

        Assert.AreEqual(
            0,
            GetPrivateField<int>(
                gestor,
                "indiceOleadaEnCurso"
            )
        );

        Assert.IsTrue(generador.OleadaEnCurso);
    }

    [Test]
    public void Update_CompletesFirstWaveAndStartsSecond()
    {
        jugadorObject.transform.position =
            new Vector3(0f, 30f, 0f);

        ExpectWaveStart(1);
        InvokeUpdate();

        SimulateWaveCompletion();

        LogAssert.Expect(
            LogType.Log,
            "GestorEventosDinamicos: Oleada 1 completada."
        );

        InvokeUpdate();

        Assert.IsTrue(IsWaveCompleted(0));

        Assert.AreEqual(
            -1,
            GetPrivateField<int>(
                gestor,
                "indiceOleadaEnCurso"
            )
        );

        jugadorObject.transform.position =
            new Vector3(0f, 60f, 0f);

        ExpectWaveStart(2);
        InvokeUpdate();

        Assert.AreEqual(
            1,
            GetPrivateField<int>(
                gestor,
                "indiceOleadaEnCurso"
            )
        );
    }

    [Test]
    public void Update_ProgressesThroughAllThreeWaves()
    {
        // Primera oleada
        jugadorObject.transform.position =
            new Vector3(0f, 30f, 0f);

        ExpectWaveStart(1);
        InvokeUpdate();

        SimulateWaveCompletion();

        LogAssert.Expect(
            LogType.Log,
            "GestorEventosDinamicos: Oleada 1 completada."
        );

        InvokeUpdate();

        Assert.IsTrue(IsWaveCompleted(0));

        // Segunda oleada
        jugadorObject.transform.position =
            new Vector3(0f, 60f, 0f);

        ExpectWaveStart(2);
        InvokeUpdate();

        SimulateWaveCompletion();

        LogAssert.Expect(
            LogType.Log,
            "GestorEventosDinamicos: Oleada 2 completada."
        );

        InvokeUpdate();

        Assert.IsTrue(IsWaveCompleted(1));

        // Tercera oleada
        jugadorObject.transform.position =
            new Vector3(0f, 85f, 0f);

        ExpectWaveStart(3);
        InvokeUpdate();

        SimulateWaveCompletion();

        LogAssert.Expect(
            LogType.Log,
            "GestorEventosDinamicos: Oleada 3 completada."
        );

        InvokeUpdate();

        Assert.IsTrue(IsWaveCompleted(2));

        Assert.AreEqual(
            -1,
            GetPrivateField<int>(
                gestor,
                "indiceOleadaEnCurso"
            )
        );
    }

    [Test]
    public void ReiniciarProgreso_ResetsCompletedWaves()
    {
        jugadorObject.transform.position =
            new Vector3(0f, 30f, 0f);

        ExpectWaveStart(1);
        InvokeUpdate();

        SimulateWaveCompletion();

        LogAssert.Expect(
            LogType.Log,
            "GestorEventosDinamicos: Oleada 1 completada."
        );

        InvokeUpdate();

        Assert.IsTrue(IsWaveCompleted(0));

        LogAssert.Expect(
            LogType.Log,
            "GestorEventosDinamicos: Progreso de oleadas restablecido."
        );

        GestorEventosDinamicos.ReiniciarProgreso();

        Assert.IsFalse(IsWaveCompleted(0));
        Assert.IsFalse(IsWaveCompleted(1));
        Assert.IsFalse(IsWaveCompleted(2));
    }

    [Test]
    public void Update_MissingPlayerOrGenerator_DoesNothing()
    {
        SetPrivateField(gestor, "jugador", null);

        Assert.DoesNotThrow(InvokeUpdate);

        SetPrivateField(gestor, "jugador", jugadorObject.transform);
        SetPrivateField(gestor, "generador", null);

        Assert.DoesNotThrow(InvokeUpdate);

        Assert.AreEqual(
            -1,
            GetPrivateField<int>(
                gestor,
                "indiceOleadaEnCurso"
            )
        );
    }
}
