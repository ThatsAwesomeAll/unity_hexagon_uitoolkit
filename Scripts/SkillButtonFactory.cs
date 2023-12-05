using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillButtonFactory : VisualElement
{
    // Define a factory class to expose this control to UXML.
    public new class UxmlFactory : UxmlFactory<SkillButtonFactory, UxmlTraits>
    {
    }

    private Sprite m_spriteSkill;

    public Sprite spriteSkill
    {
        // The progress property is exposed in C#.
        get => m_spriteSkill;
        set
        {
            m_spriteSkill = value;
            MarkDirtyRepaint();
        }
    }

    private VisualElement _mainContainer;

    public SkillButtonFactory()
    {
        _mainContainer = CreateContainer(false);
        _mainContainer.name = "mainContainer";
        Add(_mainContainer);

        VisualElement background = CreateContainer(false);
        background.name = "background";
        var sprite = Resources.Load<Sprite>("UI/HexagonSkillButton/HexagonFilledBoarder");
        background.style.backgroundImage = new StyleBackground(sprite);
        _mainContainer.Add(background);

        _mainContainer.Add(CreateMaskedSkillSprite());

        _mainContainer.Add(CreateBoarderElement());

        StyleSheet uss = Resources.Load<StyleSheet>("UI/HexagonSkillButton/StyleSheet");
        _mainContainer.styleSheets.Add(uss);
    }

    private VisualElement CreateBoarderElement()
    {
        Sprite sprite;
        HexagonVisualElement boarder = new HexagonVisualElement
        {
            style =
            {
                flexGrow = 1,
                width = Length.Percent(100),
                height = Length.Percent(100),
                position = Position.Absolute
            }
        };
        sprite = Resources.Load<Sprite>("UI/HexagonSkillButton/SkillbuttonBoarder");
        boarder.style.backgroundImage = new StyleBackground(sprite);
        boarder.name = "Boarder";
        boarder.focusable = true;
        return boarder;
    }


    private VisualElement CreateMaskedSkillSprite()
    {
        VisualElement skill = CreateContainer(true);
        skill.name = "skill";
        var svgMask = Resources.Load<VectorImage>("UI/HexagonSkillButton/HexagonSvg");
        skill.style.backgroundImage = new StyleBackground(svgMask);
        skill.style.width = Length.Percent(90);
        skill.style.height = Length.Percent(90);
        skill.style.left = Length.Percent(4);
        skill.style.top = Length.Percent(4);
        skill.style.overflow = new StyleEnum<Overflow>(Overflow.Hidden);

        VisualElement skillSprite = CreateContainer(true);
        skillSprite.name = "skillSprite";
        skillSprite.style.backgroundColor = Color.red;
        skill.Add(skillSprite);
        return skill;
    }

    VisualElement CreateContainer(bool absolute)
    {
        var container = new VisualElement();
        container.style.flexGrow = 1;
        if (absolute)
        {
            container.style.width = Length.Percent(100);
            container.style.height = Length.Percent(100);
            container.style.position = Position.Absolute;
        }

        return container;
    }
}

class HexagonVisualElement : VisualElement
{
    public override bool ContainsPoint(Vector2 localPoint)
    {
        return IsPointInHexagon(localPoint, resolvedStyle.width, resolvedStyle.height);
    }

    private enum Side
    {
        left,
        right
    }

    // assumes the hexagon is flat side at bottom
    bool IsPointInHexagon(Vector2 point, float width, float height)
    {
        // we are in middle square of object -> this is fine
        if (point.x > width * 0.25f && point.x < width * 0.75f)
        {
            return true;
        }
        // catch the triangles at the side using the 1norm -> works good for roughly uniform hexagons
        if (point.x < width * 0.25f)
        {
            return CheckQuadrantWith1Norm(Side.left, point, width, height);
        }
        if (point.x > width * 0.75f)
        {
            return CheckQuadrantWith1Norm(Side.right, point, width, height);
        }
        return false;
    }
    
    bool CheckQuadrantWith1Norm(Side side, Vector2 point, float width, float height)
    {
        float shiftCoordinatesX = 0.25f;
        if (side == Side.right)
        {
            shiftCoordinatesX = 0.75f;
        }

        const float kNormUnitCircle = 0.25f;
        const float kShiftYAxis = 0.5f;

        point.x -= width * shiftCoordinatesX;
        point.y -= height * kShiftYAxis;
        float norm_1 = Mathf.Abs(point.x) + Mathf.Abs(point.y);
        if (norm_1 <= width * kNormUnitCircle)
        {
            return true;
        }
        return false;
    }
}