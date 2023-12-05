using UnityEngine;
using UnityEngine.UIElements;


// An element that displays progress inside a partially filled circle
public class HexagonProgress : VisualElement
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
        
        UxmlFloatAttributeDescription m_LineWidthAttribute = new UxmlFloatAttributeDescription()
        {
            name = "lineWidth"
        };
        
        // Use the Init method to assign the value of the progress UXML attribute to the C# progress property.
        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);

            (ve as HexagonProgress).progress = m_ProgressAttribute.GetValueFromBag(bag, cc);
            (ve as HexagonProgress).trackColor = m_TrackColorAttribute.GetValueFromBag(bag, cc);
            (ve as HexagonProgress).progressColor = m_ProgressColorAttribute.GetValueFromBag(bag, cc);
            (ve as HexagonProgress).LineWidth = m_LineWidthAttribute.GetValueFromBag(bag, cc);
        }
    }

    // Define a factory class to expose this control to UXML.
    public new class UxmlFactory : UxmlFactory<HexagonProgress, UxmlTraits>
    {
    }

    // These are USS class names for the control overall and the label.
    public static readonly string ussClassName = "radial-progress";

    // These objects allow C# code to access custom USS properties.
    static CustomStyleProperty<Color> s_TrackColor = new CustomStyleProperty<Color>("--track-color");
    static CustomStyleProperty<Color> s_ProgressColor = new CustomStyleProperty<Color>("--progress-color");

    Color m_TrackColor = Color.red;
    Color m_ProgressColor = new Color(0.7f,0,0,1f);
    float m_Progress;
    float m_LineWidth = 2.0f;
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

    public float LineWidth
    {
        get => m_LineWidth;
        set
        {
            m_LineWidth = value;
            MarkDirtyRepaint();
        }
    }
    public Color progressColor
    {
        get => m_ProgressColor;
        set
        {
            m_ProgressColor = value;
            CreateGradient();
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
            CreateGradient();
            MarkDirtyRepaint();
        }
    }

    // This default constructor is RadialProgress's only constructor.
    public HexagonProgress()
    {
        // Create a Label, add a USS class name, and add it to this visual tree.
        style.flexGrow = new StyleFloat(1);
        
        CreateGradient();

        // Add the USS class name for the overall control.
        AddToClassList(ussClassName);

        // Register a callback after custom style resolution.
        RegisterCallback<CustomStyleResolvedEvent>(evt => CustomStylesResolved(evt));

        // Register a callback to generate the visual content of the control.
        generateVisualContent += GenerateVisualContent;

        progress = 0.0f;
    }

    private void CreateGradient()
    {
        // Blend color from red at 0% to blue at 100%
        var colors = new GradientColorKey[5];
        colors[0] = new GradientColorKey(progressColor, 0.0f);
        colors[1] = new GradientColorKey(trackColor, .25f);
        colors[2] = new GradientColorKey(progressColor, 0.5f);
        colors[3] = new GradientColorKey(trackColor, .75f);
        colors[4] = new GradientColorKey(progressColor, 1.0f);

        // Blend color from red at 0% to blue at 100%
        var alpha = new GradientAlphaKey[5];
        alpha[0] = new GradientAlphaKey(progressColor.a, 0.0f);
        alpha[1] = new GradientAlphaKey(trackColor.a, .25f);
        alpha[2] = new GradientAlphaKey(progressColor.a, 0.5f);
        alpha[3] = new GradientAlphaKey(trackColor.a, .75f);
        alpha[4] = new GradientAlphaKey(progressColor.a, 1.0f);

        _gradient.alphaKeys = alpha;
        _gradient.colorKeys = colors;
    }

    static void CustomStylesResolved(CustomStyleResolvedEvent evt)
    {
        HexagonProgress element = (HexagonProgress)evt.currentTarget;
        element.UpdateCustomStyles();
    }

    // After the custom colors are resolved, this method uses them to color the meshes and (if necessary) repaint
    // the control.
    void UpdateCustomStyles()
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

        PaintHexagon(width, height, progressColor, context, progress);
    }

    void PaintHexagon(float width, float height, Color color, MeshGenerationContext context, float hexagonFillPercent)
    {
        var painter = context.painter2D;
       
        Vector2[] vertices = Hexagon.GetHexagonVertices(width, height, _center);
        float percentagePerSegment = 100.0f / 6.0f;

        for (int i = 0; i < 6; i++)
        {
            float shortenPercentage = 0.8f;
            if (!ComputeShortenPercentage(ref shortenPercentage, i))
            {
                continue;
            }

            var shortLine = Hexagon.ShortenLineCenter(shortenPercentage,
                new Hexagon.LineVector2() { start = vertices[i], end = vertices[(i + 1) % 6] });
            DrawThickLine(shortLine.start, shortLine.end, color, LineWidth, painter);
        }

        bool ComputeShortenPercentage(ref float percentage, int hexagonIndex)
        {
            if ((percentagePerSegment * (hexagonIndex + 1)) > hexagonFillPercent) // not needed segments
            {
                if ((percentagePerSegment * hexagonIndex) < hexagonFillPercent) // last segment which we should draw in
                {
                    var remainingPercent = hexagonFillPercent - percentagePerSegment * hexagonIndex;
                    var fillPercent = remainingPercent / percentagePerSegment;
                    percentage *= fillPercent;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    }


    void DrawThickLine(Vector2 start, Vector2 end, Color color, float lineWidth, Painter2D painter)
    {
        Vector2 dir = end - start;
        Vector2 dirNormalized = (end - start).normalized;
        Vector2 inwardNormal = new Vector2(dir.y, -dir.x).normalized;
        painter.strokeColor = color;
        painter.strokeGradient = _gradient;
        painter.lineWidth = lineWidth;
        
        var shift = lineWidth;
        if (shift > dir.magnitude *0.5f)
        {
            shift = dir.magnitude * 0.5f;
        }

        
        painter.BeginPath();
        painter.MoveTo(start + 0.5f * dir);
        painter.LineTo(start);
        painter.LineTo(start + inwardNormal * shift + dirNormalized * shift);
        painter.LineTo(end + inwardNormal * shift - dirNormalized * shift);
        painter.LineTo(end);
        painter.LineTo(end - 0.5f * dir);
        painter.Stroke(); 

    }
}

class Hexagon
{
    
    
    public struct LineVector2
    {
        public Vector2 start;
        public Vector2 end;
    }


   static public Vector2[] GetHexagonVertices(float width, float height, Vector2 center, float shiftAngle = 0.0f)
   {
       float radius = height < width ? height * 0.5f : width * 0.5f;

       // Calculate the vertices of the hexagon
       Vector2 centerHex = new Vector2(width * 0.5f, height * 0.5f) + center;
       Vector2[] vertices = new Vector2[6];
       float shiftToTop =  - Mathf.Deg2Rad * 120.0f;
       for (int i = 0; i < 6; i++)
       {
           float angle = 2 * Mathf.PI / 6 * i + shiftAngle + shiftToTop;
           vertices[i] = centerHex + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
       }

       return vertices;
   }

   static public  LineVector2 ShortenLineCenter(float percentage, LineVector2 line)
   {
       Vector2 direction = line.end - line.start;
       float distance = direction.magnitude;
       float shortenedDistance = distance * (1 - percentage);

       Vector2 shortenedDirection = direction.normalized * shortenedDistance;

       LineVector2 shortenedLine;
       shortenedLine.start = line.start + shortenedDirection / 2;
       shortenedLine.end = line.end - shortenedDirection / 2;

       return shortenedLine;
   }

   public static LineVector2 SwapLinetoGoFromBottomtoTop(Hexagon.LineVector2 line)
   {
       if (line.start.y < line.end.y)
       {
           var tempend = line.end;
           line.end = line.start;
           line.start = tempend;
       }

       return line;
   }


   public static LineVector2 ShortenLineFromEnding(float percentage, LineVector2 line, bool cutFromEnd = true)
   {
       Vector2 direction = line.end - line.start;
       float distance = direction.magnitude;
       float shortenedDistance = distance * (1 - percentage);

       Vector2 shortenedDirection = direction.normalized * shortenedDistance;

       LineVector2 shortenedLine;
       if (cutFromEnd)
       {
           shortenedLine.start = line.start;
           shortenedLine.end = line.end - shortenedDirection;
       }
       else
       {
           shortenedLine.start = line.start + shortenedDirection;
           shortenedLine.end = line.end;
       }

       return shortenedLine;
   }
}