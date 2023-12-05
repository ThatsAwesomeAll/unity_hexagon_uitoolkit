using UnityEngine;
using UnityEngine.UIElements;


// An element that displays progress inside a partially filled circle
public class HexagonFilled : VisualElement
{
    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        // The progress property is exposed to UXML.
        UxmlFloatAttributeDescription m_ProgressAttribute = new UxmlFloatAttributeDescription()
        {
            name = "progress"
        };

        UxmlColorAttributeDescription m_TrackColorAttribute = new UxmlColorAttributeDescription()
        {
            name = "trackColor"
        };

        UxmlColorAttributeDescription m_ProgressColorAttribute = new UxmlColorAttributeDescription()
        {
            name = "progressColor"
        };

        // Use the Init method to assign the value of the progress UXML attribute to the C# progress property.
        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);

            (ve as HexagonFilled).progress = m_ProgressAttribute.GetValueFromBag(bag, cc);
            (ve as HexagonFilled).trackColor = m_TrackColorAttribute.GetValueFromBag(bag, cc);
            (ve as HexagonFilled).progressColor = m_ProgressColorAttribute.GetValueFromBag(bag, cc);
        }
    }

    // Define a factory class to expose this control to UXML.
    public new class UxmlFactory : UxmlFactory<HexagonFilled, UxmlTraits>
    {
    }

    // These are USS class names for the control overall and the label.
    public static readonly string ussClassName = "hexagon-fill-progress";

    // These objects allow C# code to access custom USS properties.
    static CustomStyleProperty<Color> s_TrackColor = new CustomStyleProperty<Color>("--track-color");
    static CustomStyleProperty<Color> s_ProgressColor = new CustomStyleProperty<Color>("--progress-color");

    Color m_TrackColor = Color.red;
    Color m_ProgressColor = new Color(0.7f, 0, 0, 1f);
    float m_Progress;
    private Vector2 _center;
    private Gradient _gradient = new Gradient();

    public Color trackColor
    {
        // The progress property is exposed in C#.
        get => m_TrackColor;
        set
        {
            m_TrackColor = value;
            MarkDirtyRepaint();
        }
    }

    public Color progressColor
    {
        get => m_ProgressColor;
        set
        {
            m_ProgressColor = value;
            MarkDirtyRepaint();
        }
    }

    // A value between 0 and 100
    public float progress
    {
        get => m_Progress;
        set
        {
            m_Progress = value;
            MarkDirtyRepaint();
        }
    }

    // This default constructor is RadialProgress's only constructor.
    public HexagonFilled()
    {
        
        style.flexGrow = new StyleFloat(1);

        // Add the USS class name for the overall control.
        AddToClassList(ussClassName);

        // Register a callback after custom style resolution.
        RegisterCallback<CustomStyleResolvedEvent>(evt => CustomStylesResolved(evt));

        // Register a callback to generate the visual content of the control.
        generateVisualContent += GenerateVisualContent;

        progress = 0.0f;
    }

    static void CustomStylesResolved(CustomStyleResolvedEvent evt)
    {
        HexagonFilled element = (HexagonFilled)evt.currentTarget;
        element.UpdateCustomStylesHexFilled();
    }

    // After the custom colors are resolved, this method uses them to color the meshes and (if necessary) repaint
    // the control.
    void UpdateCustomStylesHexFilled()
    {
        bool repaint = false;
        if (customStyle.TryGetValue(s_ProgressColor, out m_ProgressColor))
            repaint = true;

        if (customStyle.TryGetValue(s_TrackColor, out m_TrackColor))
            repaint = true;

        if (repaint)
            MarkDirtyRepaint();
    }

    void GenerateVisualContent(MeshGenerationContext context)
    {
        float width = contentRect.width;
        float height = contentRect.height;
        _center = new Vector2(contentRect.x, contentRect.y);
        float progressFactor = progress > 100.0f? 100.0f : progress;
        progressFactor *= 0.01f;
        PaintHexagon(width, height, context, progressFactor);
    }

    void PaintHexagon(float width, float height, MeshGenerationContext context, float hexagonFillPercent)
    {
        Vector2[] vertices = Hexagon.GetHexagonVertices(width, height, _center, Mathf.Deg2Rad * 1800f);
        // cmopute lowest and highes point on the hexagon to scale properly
        float radius = height < width ? height * 0.5f : width * 0.5f;
        float heightHexagonTriangle = radius * Mathf.Cos(30 * Mathf.Deg2Rad);
        float heightDiff = radius - heightHexagonTriangle - 1f; // add single pixel solving float percision issues
        float startPosY = heightDiff + _center.y;
        float maxPosY = height - heightDiff + _center.y;
        float cutPosY = startPosY + (maxPosY - startPosY) * (1 - hexagonFillPercent);
        
        var painter = context.painter2D;
        painter.strokeColor = trackColor;
        painter.fillColor = progressColor;
        painter.lineWidth = 1.0f;
        painter.BeginPath();
        bool firstDraw = false;
        for (int i_vertex = 0; i_vertex < 6; i_vertex++)
        {
            Hexagon.LineVector2 line = new Hexagon.LineVector2();
            line.end = vertices[(i_vertex + 1) % 6];
            line.start = vertices[i_vertex];
            if (LineOutsideFillArea(line,cutPosY))
            {
                continue;
            }
            if (!LineCompletelyInsideFillArea(line, cutPosY))
            {
                bool cutFromEnd = !(line.start.y < line.end.y);
                float yMin =  Mathf.Min(line.end.y, line.start.y);
                float yMax =  Mathf.Max(line.end.y, line.start.y);
                float cutFactor = (cutPosY - yMin) / (yMax - yMin);
                line = Hexagon.ShortenLineFromEnding((1-cutFactor), line, cutFromEnd);
            }
            if (firstDraw == false)
            {
                painter.MoveTo(line.start);
                firstDraw = true;
            }
            painter.LineTo(line.end);
        }
        painter.Fill();
        painter.Stroke();
    }

    bool LineOutsideFillArea(Hexagon.LineVector2 line, float cutPosY)
    {
        return line.start.y <= cutPosY && line.end.y <= cutPosY;
    }    
    bool LineCompletelyInsideFillArea(Hexagon.LineVector2 line, float cutPosY)
    {
        return line.start.y >= cutPosY && line.end.y >= cutPosY;
    }
}