using UnityEngine;
using UnityEngine.UI;

// Resolution-independent artwork: no icon font or bitmap required.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class BingoIcon : MaskableGraphic
{
    public enum Shape { Mark, Arrow, Download, Flag, Diamond, Owl }
    public Shape Kind;
    private VertexHelper _mesh;

    protected override void Awake()
    {
        base.Awake();
        useLegacyMeshGeneration = false;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        _mesh = vh;
        switch (Kind)
        {
            case Shape.Mark:
                Triangle(new Vector2(0, 1), Vector2.one, new Vector2(1, 0), color);
                Line(.59f, .73f, .7f, .62f, .055f, Color.white);
                Line(.7f, .62f, .88f, .84f, .055f, Color.white);
                break;
            case Shape.Arrow:
                Line(.08f, .5f, .94f, .5f); Line(.08f, .5f, .4f, .82f); Line(.08f, .5f, .4f, .18f); break;
            case Shape.Download:
                Line(.5f, .95f, .5f, .32f); Line(.5f, .32f, .24f, .59f); Line(.5f, .32f, .76f, .59f);
                Line(.12f, .29f, .12f, .08f); Line(.12f, .08f, .88f, .08f); Line(.88f, .08f, .88f, .29f); break;
            case Shape.Flag:
                Line(.16f, .05f, .16f, .96f); Line(.16f, .9f, .48f, .96f); Line(.48f, .96f, .84f, .84f);
                Line(.84f, .84f, .84f, .4f); Line(.84f, .4f, .48f, .5f); Line(.48f, .5f, .16f, .43f); break;
            case Shape.Diamond:
                Triangle(new Vector2(.5f, 1), new Vector2(1, .5f), new Vector2(.5f, 0), color);
                Triangle(new Vector2(.5f, 1), new Vector2(.5f, 0), new Vector2(0, .5f), color); break;
            case Shape.Owl:
                Circle(.5f, .5f, .49f, color);
                Color light = BingoStyle.Paper;
                Line(.2f, .82f, .27f, .53f, .025f, light); Line(.2f, .82f, .5f, .68f, .025f, light);
                Line(.8f, .82f, .73f, .53f, .025f, light); Line(.8f, .82f, .5f, .68f, .025f, light);
                Ring(.35f, .58f, .145f, light); Ring(.65f, .58f, .145f, light);
                Circle(.35f, .58f, .055f, light); Circle(.65f, .58f, .055f, light);
                Triangle(new Vector2(.45f, .48f), new Vector2(.55f, .48f), new Vector2(.5f, .37f), light);
                Line(.23f, .4f, .3f, .2f, .025f, light); Line(.3f, .2f, .5f, .12f, .025f, light);
                Line(.77f, .4f, .7f, .2f, .025f, light); Line(.7f, .2f, .5f, .12f, .025f, light);
                for (int i = 0; i < 3; i++)
                {
                    float y = .34f - i * .065f;
                    Line(.38f, y, .5f, y - .045f, .018f, light); Line(.5f, y - .045f, .62f, y, .018f, light);
                }
                break;
        }
    }

    private Vector2 Point(Vector2 p) => rectTransform.rect.min + Vector2.Scale(p, rectTransform.rect.size);
    private void Triangle(Vector2 a, Vector2 b, Vector2 c, Color tint)
    {
        int index = _mesh.currentVertCount;
        _mesh.AddVert(Point(a), tint, Vector2.zero); _mesh.AddVert(Point(b), tint, Vector2.zero); _mesh.AddVert(Point(c), tint, Vector2.zero);
        _mesh.AddTriangle(index, index + 1, index + 2);
    }
    private void Line(float x1, float y1, float x2, float y2, float thickness = .065f, Color? tint = null)
    {
        var a = new Vector2(x1, y1); var b = new Vector2(x2, y2);
        var normal = new Vector2(-(b - a).y, (b - a).x).normalized * thickness / 2;
        Triangle(a - normal, a + normal, b + normal, tint ?? color);
        Triangle(a - normal, b + normal, b - normal, tint ?? color);
    }
    private void Circle(float x, float y, float radius, Color tint)
    {
        for (int i = 0; i < 64; i++)
        {
            float a = i * Mathf.PI / 32, b = (i + 1) * Mathf.PI / 32;
            Triangle(new Vector2(x, y), new Vector2(x + Mathf.Cos(a) * radius, y + Mathf.Sin(a) * radius),
                new Vector2(x + Mathf.Cos(b) * radius, y + Mathf.Sin(b) * radius), tint);
        }
    }
    private void Ring(float x, float y, float radius, Color tint)
    {
        for (int i = 0; i < 48; i++)
        {
            float a = i * Mathf.PI / 24, b = (i + 1) * Mathf.PI / 24;
            Line(x + Mathf.Cos(a) * radius, y + Mathf.Sin(a) * radius,
                x + Mathf.Cos(b) * radius, y + Mathf.Sin(b) * radius, .018f, tint);
        }
    }
}
