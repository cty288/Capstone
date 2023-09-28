/// Credit Simie
/// Sourced from - http://forum.unity3d.com/threads/flowlayoutgroup.296709/
/// Example http://forum.unity3d.com/threads/flowlayoutgroup.296709/
/// Update by Martin Sharkbomb - http://forum.unity3d.com/threads/flowlayoutgroup.296709/#post-1977028
/// Last item alignment fix by Vicente Russo - https://bitbucket.org/ddreaper/unity-ui-extensions/issues/22/flow-layout-group-align
/// Vertical Flow by Ramon Molossi
/// Children auto-resize by Fidel Soto
 
using System.Collections.Generic;
 
namespace UnityEngine.UI.Extensions
{
    /// <summary>
    /// Layout Group controller that arranges children in bars, fitting as many on a line until total size exceeds parent bounds
    /// </summary>
    [AddComponentMenu("Layout/Extensions/Dynamic Grid Layout Group")]
    public class DynamicGridLayoutGroup : LayoutGroup
    {
 public float fixedRowHeight = 50f; // Fixed height for each row
    public float spacing = 5f; // Spacing between elements

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        float totalHeight = padding.top + padding.bottom;
        float currentRowWidth = 0;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            RectTransform child = rectChildren[i];
            float newWidth = currentRowWidth + child.rect.width + spacing;
            if (newWidth > rectTransform.rect.width - padding.left - padding.right)
            {
                totalHeight += fixedRowHeight + spacing;
                currentRowWidth = 0;
            }
            currentRowWidth += child.rect.width + spacing;
        }

        totalHeight += fixedRowHeight; // Add height for the last row
        SetLayoutInputForAxis(totalHeight, totalHeight, -1, 1);
    }

    public override void CalculateLayoutInputVertical() {
        
    }
    
    

    public override void SetLayoutHorizontal()
    {
        float xOffset = padding.left;
        float currentRowWidth = 0;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            RectTransform child = rectChildren[i];
            if (currentRowWidth + child.rect.width + spacing > rectTransform.rect.width - padding.left - padding.right)
            {
                xOffset = padding.left;
                currentRowWidth = 0;
            }

            SetChildAlongAxis(child, 0, xOffset);
            xOffset += child.rect.width + spacing;
            currentRowWidth += child.rect.width + spacing;
        }
    }

    public override void SetLayoutVertical()
    {
        float yOffset = padding.top;
        float currentRowWidth = 0;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            RectTransform child = rectChildren[i];
            if (currentRowWidth + child.rect.width + spacing > rectTransform.rect.width - padding.left - padding.right)
            {
                yOffset += fixedRowHeight + spacing;
                currentRowWidth = 0;
            }

            SetChildAlongAxis(child, 1, yOffset, fixedRowHeight);
            currentRowWidth += child.rect.width + spacing;
        }
    }
    }
}