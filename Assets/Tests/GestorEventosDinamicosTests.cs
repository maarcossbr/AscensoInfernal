
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
    private GameObject metaObject;
    private GameObject camaraObject;

    private GestorEventosDinamicos gestor;
    private GeneradorMeteoritos generador;
    private MeteoritoPool pool;
    private Meteorito prefab;
    private Camara camara;

    [SetUp]
    public void SetUp()
    {
        // Restablecer el progreso estatico
        LogAssert.Expect(
            LogType.Log,
            "GestorEventosDinamicos: Progreso de oleadas restablecido."
        );

        GestorEventosDinamicos.ReiniciarProgreso();

        // Crear jugador
        jugadorObject = new GameObject("Player_Test");

        // Crear meta
        metaObject = new GameObject("Meta_Test");
        metaObject.transform.position = new Vector3(0f, 100f, 0f);

        // Crear camara
        camaraObject = new GameObject("Camara_Test");
        camara = camaraObject.AddComponent<Camara>();

        // Crear prefab de meteorito
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
        SetPrivateField(generador, "camara", null);

        // El generador necesita una camara de Unity,
        // no el componente Camara de seguimiento.
        Camera unityCamera = camaraObject.AddComponent<Camera>();
        unityCamera.orthographic = true;

        SetPrivateField(generador, "camara", unityCamera);

        // Crear gestor
        gestorObject = new GameObject("Gestor_Test");
        gestor = gestorObject.AddComponent<GestorEventosDinamicos>();

        SetPrivateField(gestor, "jugador", jugadorObject.transform);
        SetPrivateField(gestor, "metaNivel", metaObject.transform);
        SetPrivateField(gestor, "generador", generador);
        SetPrivateField(gestor, "camara", camara);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gestorObject);
        Object.DestroyImmediate(generadorObject);
        Object.DestroyImmediate(poolObject);
        Object.DestroyImmediate(prefabObject);
        Object.DestroyImmediate(jugadorObject);
        Object.DestroyImmediate(metaObject);
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

    private object GetPrivateField(
        object target,
        string fieldName)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(field);

        return field.GetValue(target);
    }

    private object InvokePrivateMethod(string methodName)
    {
        MethodInfo method = typeof(GestorEventosDinamicos).GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(method);

        return method.Invoke(gestor, null);
    }

    private void ExpectWaveStart(int number)
    {
        LogAssert.Expect(
            LogType.Log,
            "GeneradorMeteoritos: Comienza la lluvia de meteoritos."
        );

        LogAssert.Expect(
            LogType.Log,
            "Meteoritos generados durante la oleada: 1"
        );

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

    private void ExpectWaveEnd(int number)
    {
        LogAssert.Expect(
            LogType.Log,
            "GeneradorMeteoritos: Oleada detenida."
        );

        LogAssert.Expect(
            LogType.Log,
            "GestorEventosDinamicos: Oleada "
            + number + " completada."
        );
    }

    private bool IsWaveCompleted(int index)
    {
        MethodInfo method = typeof(GestorEventosDinamicos).GetMethod(
            "EstaCompletada",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(method);

        return (bool)method.Invoke(gestor, new object[] { index });
    }

    [Test]
    public void Update_BelowFirstThreshold_DoesNotStartWave()
    {
        jugadorObject.transform.position = new Vector3(0f, 29f, 0f);

        InvokePrivateMethod("Update");

        Assert.AreEqual(
            -1,
            (int)GetPrivateField(gestor, "indiceOleadaEnCurso")
        );

        Assert.AreEqual(1, pool.ObtenerCantidadDisponible());
    }

    [Test]
    public void Update_StartsFirstWaveAtRequiredHeight()
    {
        jugadorObject.transform.position = new Vector3(0f, 30f, 0f);

        ExpectWaveStart(1);

        InvokePrivateMethod("Update");

        Assert.AreEqual(
            0,
            (int)GetPrivateField(gestor, "indiceOleadaEnCurso")
        );

        Assert.AreEqual(0, pool.ObtenerCantidadDisponible());
    }

    [Test]
    public void Update_DoesNotStartAnotherWaveWhileOneIsActive()
    {
        jugadorObject.transform.position = new Vector3(0f, 30f, 0f);

        ExpectWaveStart(1);

        InvokePrivateMethod("Update");

        jugadorObject.transform.position = new Vector3(0f, 45f, 0f);

        InvokePrivateMethod("Update");

        Assert.AreEqual(
            0,
            (int)GetPrivateField(gestor, "indiceOleadaEnCurso")
        );
    }

    [Test]
    public void Update_CompletesFirstWaveAtSecondThreshold()
    {
        jugadorObject.transform.position = new Vector3(0f, 30f, 0f);

        ExpectWaveStart(1);

        InvokePrivateMethod("Update");

        jugadorObject.transform.position = new Vector3(0f, 60f, 0f);

        ExpectWaveEnd(1);

        InvokePrivateMethod("Update");

        Assert.IsTrue(IsWaveCompleted(0));

        Assert.AreEqual(
            -1,
            (int)GetPrivateField(gestor, "indiceOleadaEnCurso")
        );
    }

    [Test]
    public void Update_ProgressesThroughAllThreeWaves()
    {
        // Primera oleada
        jugadorObject.transform.position = new Vector3(0f, 30f, 0f);

        ExpectWaveStart(1);
        InvokePrivateMethod("Update");

        // Final de la primera oleada
        jugadorObject.transform.position = new Vector3(0f, 60f, 0f);

        ExpectWaveEnd(1);
        InvokePrivateMethod("Update");

        Assert.IsTrue(IsWaveCompleted(0));

        // Segunda oleada
        ExpectWaveStart(2);
        InvokePrivateMethod("Update");

        // Final de la segunda oleada
        jugadorObject.transform.position = new Vector3(0f, 85f, 0f);

        ExpectWaveEnd(2);
        InvokePrivateMethod("Update");

        Assert.IsTrue(IsWaveCompleted(1));

        // Tercera oleada
        ExpectWaveStart(3);
        InvokePrivateMethod("Update");

        // Final de la tercera oleada: la meta esta en y = 100
        jugadorObject.transform.position = new Vector3(0f, 100f, 0f);

        ExpectWaveEnd(3);
        InvokePrivateMethod("Update");

        Assert.IsTrue(IsWaveCompleted(2));

        Assert.AreEqual(
            -1,
            (int)GetPrivateField(gestor, "indiceOleadaEnCurso")
        );
    }

    [Test]
    public void ReiniciarProgreso_ResetsCompletedWaves()
    {
        jugadorObject.transform.position = new Vector3(0f, 30f, 0f);

        ExpectWaveStart(1);
        InvokePrivateMethod("Update");

        jugadorObject.transform.position = new Vector3(0f, 60f, 0f);

        ExpectWaveEnd(1);
        InvokePrivateMethod("Update");

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
    public void Update_LastWaveWithoutGoal_DoesNotCompleteWave()
    {
        jugadorObject.transform.position = new Vector3(0f, 30f, 0f);

        ExpectWaveStart(1);
        InvokePrivateMethod("Update");

        jugadorObject.transform.position = new Vector3(0f, 60f, 0f);

        ExpectWaveEnd(1);
        InvokePrivateMethod("Update");

        ExpectWaveStart(2);
        InvokePrivateMethod("Update");

        jugadorObject.transform.position = new Vector3(0f, 85f, 0f);

        ExpectWaveEnd(2);
        InvokePrivateMethod("Update");

        ExpectWaveStart(3);
        InvokePrivateMethod("Update");

        // Quitar la referencia a la meta
        SetPrivateField(gestor, "metaNivel", null);

        LogAssert.Expect(
            LogType.Error,
            "GestorEventosDinamicos: No se ha asignado la meta."
        );

        jugadorObject.transform.position = new Vector3(0f, 100f, 0f);

        InvokePrivateMethod("Update");

        Assert.IsFalse(IsWaveCompleted(2));

        Assert.AreEqual(
            2,
            (int)GetPrivateField(gestor, "indiceOleadaEnCurso")
        );
    }
}
