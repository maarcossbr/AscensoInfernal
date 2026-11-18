
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MeteoritoPoolTests
{
    private GameObject poolObject;
    private GameObject prefabObject;

    private MeteoritoPool pool;
    private Meteorito prefab;

    [SetUp]
    public void SetUp()
    {
        // Crear prefab de meteorito
        prefabObject = new GameObject("Meteorito_Prefab_Test");

        prefabObject.AddComponent<Rigidbody2D>();
        prefab = prefabObject.AddComponent<Meteorito>();

        prefabObject.SetActive(false);

        // Crear pool inicialmente desactivado
        poolObject = new GameObject("MeteoritoPool_Test");
        poolObject.SetActive(false);

        pool = poolObject.AddComponent<MeteoritoPool>();

        // Configurar campos privados antes de ejecutar Awake
        SetPrivateField("prefabMeteorito", prefab);
        SetPrivateField("cantidadInicial", 1);
        SetPrivateField("maximoMeteoritos", 2);

        // Activar el objeto para inicializar el pool
        LogAssert.Expect(
            LogType.Log,
            "MeteoritoPool inicializado. Meteoritos creados: 1"
        );

        poolObject.SetActive(true);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(poolObject);
        Object.DestroyImmediate(prefabObject);
    }

    private void SetPrivateField(string fieldName, object value)
    {
        FieldInfo field = typeof(MeteoritoPool).GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(
            field,
            "No se ha encontrado el campo " + fieldName
        );

        field.SetValue(pool, value);
    }

    [Test]
    public void InicializarPool_CreatesInitialMeteorites()
    {
        Assert.AreEqual(1, pool.ObtenerCantidadTotal());
        Assert.AreEqual(1, pool.ObtenerCantidadDisponible());
    }

    [Test]
    public void Obtener_ReturnsAndActivatesMeteorite()
    {
        Meteorito meteorito = pool.Obtener(
            new Vector3(3f, 10f, 0f),
            8f,
            2
        );

        Assert.IsNotNull(meteorito);
        Assert.IsTrue(meteorito.gameObject.activeSelf);

        Assert.AreEqual(
            new Vector3(3f, 10f, 0f),
            meteorito.transform.position
        );

        Assert.AreEqual(0, pool.ObtenerCantidadDisponible());
        Assert.AreEqual(1, pool.ObtenerCantidadTotal());
    }

    [Test]
    public void Obtener_CreatesNewMeteoriteWhenPoolIsEmpty()
    {
        Meteorito primero = pool.Obtener(
            Vector3.zero,
            8f,
            1
        );

        Meteorito segundo = pool.Obtener(
            Vector3.one,
            8f,
            1
        );

        Assert.IsNotNull(primero);
        Assert.IsNotNull(segundo);
        Assert.AreNotSame(primero, segundo);

        Assert.AreEqual(2, pool.ObtenerCantidadTotal());
        Assert.AreEqual(0, pool.ObtenerCantidadDisponible());
    }

    [Test]
    public void Obtener_ReturnsNullWhenMaximumIsReached()
    {
        pool.Obtener(Vector3.zero, 8f, 1);
        pool.Obtener(Vector3.one, 8f, 1);

        LogAssert.Expect(
            LogType.Warning,
            "MeteoritoPool: Se ha alcanzado el limite de meteoritos."
        );

        Meteorito tercero = pool.Obtener(
            Vector3.one * 2f,
            8f,
            1
        );

        Assert.IsNull(tercero);
        Assert.AreEqual(2, pool.ObtenerCantidadTotal());
    }

    [Test]
    public void Devolver_AddsMeteoriteToPool()
    {
        Meteorito meteorito = pool.Obtener(
            Vector3.zero,
            8f,
            1
        );

        pool.Devolver(meteorito);

        Assert.IsFalse(meteorito.gameObject.activeSelf);
        Assert.AreEqual(1, pool.ObtenerCantidadDisponible());
    }

    [Test]
    public void Obtener_ReusesPreviouslyReturnedMeteorite()
    {
        Meteorito primero = pool.Obtener(
            Vector3.zero,
            8f,
            1
        );

        pool.Devolver(primero);

        Meteorito segundo = pool.Obtener(
            new Vector3(5f, 5f, 0f),
            6f,
            2
        );

        Assert.AreSame(primero, segundo);
        Assert.IsTrue(segundo.gameObject.activeSelf);

        Assert.AreEqual(1, pool.ObtenerCantidadTotal());
        Assert.AreEqual(0, pool.ObtenerCantidadDisponible());
    }

    [Test]
    public void Devolver_DoesNotAddSameMeteoriteTwice()
    {
        Meteorito meteorito = pool.Obtener(
            Vector3.zero,
            8f,
            1
        );

        pool.Devolver(meteorito);
        pool.Devolver(meteorito);

        Assert.AreEqual(1, pool.ObtenerCantidadDisponible());
        Assert.AreEqual(1, pool.ObtenerCantidadTotal());
    }

    [Test]
    public void Devolver_IgnoresNullMeteorite()
    {
        pool.Devolver(null);

        Assert.AreEqual(1, pool.ObtenerCantidadDisponible());
        Assert.AreEqual(1, pool.ObtenerCantidadTotal());
    }
}
