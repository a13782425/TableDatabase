using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[Serializable]
public class DivisionSlider : IEnumerable<float>
{

    public float sliderSize;
    /// <summary>
    /// If true, inner divisions will push and pull outer divisions
    /// </summary>
    public bool pushDivisions;

    public enum ResizeMode { PrioritizeOuter, DistributeSpace }

    [SerializeField]
    private float[] offsets;
    [SerializeField]
    private float[] minSizes;
    [SerializeField]
    private float[] maxSizes;

    public DivisionSlider(int divisionCount, float sliderSize, bool pushDivisions = false)
    {
        if (divisionCount < 2) throw new ArgumentOutOfRangeException("divisionCount", divisionCount, "There must be at least 2 sections");

        offsets = new float[divisionCount];
        minSizes = new float[divisionCount];
        maxSizes = new float[divisionCount];

        this.sliderSize = sliderSize;
        this.pushDivisions = pushDivisions;
    }

    public DivisionSlider(float sliderSize, bool pushDivisions = false, params float[] sizes) : this(sizes.Length, sliderSize, pushDivisions)
    {
        float offset = 0f;
        for (int i = 0; i < sizes.Length; i++)
        {
            offsets[i] = (offset += sizes[i]);
            minSizes[i] = 10f;
            maxSizes[i] = float.MaxValue;
        }
    }

    public DivisionSlider(float[] sizes, float[] min, float[] max, float sliderSize, bool pushDivisions = false) : this(sizes.Length, sliderSize, pushDivisions)
    {
        float offset = 0f;
        for (int i = 0; i < sizes.Length; i++)
        {
            offsets[i] = (offset += sizes[i]);
            minSizes[i] = min[i];
            maxSizes[i] = max[i];
        }
    }

    public float this[int index]
    {
        get
        {
            return GetSize(index);
        }
        set
        {
            SetSize(index, value);
        }
    }

    public float GetOffset(int index)
    {
        return offsets[index];
    }

    public void SetOffset(int index, float offset)
    {
        float deltaOffset = offset - offsets[index];
        Slide(index, deltaOffset);
    }

    public float GetSize(int index)
    {
        if (index == 0) return offsets[0];
        return (offsets[index] - offsets[index - 1]);
    }

    public void SetSize(int index, float size)
    {
        float deltaSize = size - GetSize(index);
        Slide(index, deltaSize);
    }


    public float[] MaxSizes
    {
        get
        {
            return maxSizes;
        }
    }

    public float[] MinSizes
    {
        get
        {
            return minSizes;
        }
    }

    public void SetMinSize(float minSize)
    {
        for (int i = 0; i < minSizes.Length; i++)
        {
            minSizes[i] = minSize;
        }
    }

    public void SetMaxSize(float maxSize)
    {
        for (int i = 0; i < maxSizes.Length; i++)
        {
            maxSizes[i] = maxSize;
        }
    }

    public void Resize(float newSize, ResizeMode resizeMode = ResizeMode.PrioritizeOuter)
    {
        float resizeDelta = newSize - offsets[offsets.Length - 1];

        if (resizeMode == ResizeMode.PrioritizeOuter)
        {
            if (resizeDelta > 0)
            {
                for (int i = offsets.Length - 1; i >= 0; i--)
                {
                    float offsetDelta = Mathf.Min(maxSizes[i] - GetSize(i), resizeDelta);
                    PushFrom(i, offsetDelta);
                    resizeDelta -= offsetDelta;
                    if (resizeDelta <= 0) break;
                }
            }
            else
            {
                for (int i = offsets.Length - 1; i >= 0; i--)
                {
                    float offsetDelta = Mathf.Max(minSizes[i] - GetSize(i), resizeDelta);
                    PushFrom(i, offsetDelta);
                    resizeDelta -= offsetDelta;
                    if (resizeDelta >= 0) break;
                }
            }
        }
        else
        {// DistributeSpace
            int divisionsLeft = offsets.Length;
            if (resizeDelta > 0)
            {
                while (resizeDelta > 0 && divisionsLeft > 0)
                {
                    float stepResizeDelta = resizeDelta / divisionsLeft;
                    for (int i = offsets.Length - 1; i >= 0; i--)
                    {
                        float offsetDelta = Mathf.Min(maxSizes[i] - GetSize(i), stepResizeDelta);
                        PushFrom(i, offsetDelta);
                        resizeDelta -= offsetDelta;
                        if (offsetDelta <= stepResizeDelta) divisionsLeft--;
                    }
                }
            }
            else
            {
                while (resizeDelta < 0 && divisionsLeft > 0)
                {
                    float stepResizeDelta = resizeDelta / divisionsLeft;
                    for (int i = offsets.Length - 1; i >= 0; i--)
                    {
                        float offsetDelta = Mathf.Max(minSizes[i] - GetSize(i), stepResizeDelta);
                        PushFrom(i, offsetDelta);
                        resizeDelta -= offsetDelta;
                        if (offsetDelta >= stepResizeDelta) divisionsLeft--;
                    }
                }
            }
        }
    }

    private void PushFrom(int index, float delta)
    {
        for (int i = index; i < offsets.Length; i++)
        {
            offsets[i] += delta;
        }
    }

    private void Slide(int index, float delta)
    {
        float newOffset = offsets[index] + delta;

        if (delta > 0)
        {
            float maxOffset = index == 0 ? maxSizes[index] : offsets[index - 1] + maxSizes[index];
            if (!pushDivisions && index < offsets.Length - 1)
            {
                maxOffset = Mathf.Min(maxOffset, offsets[index + 1] - minSizes[index + 1]);
            }
            if (newOffset > maxOffset) newOffset = maxOffset;
        }
        else
        {
            float minOffset = index == 0 ? minSizes[index] : offsets[index - 1] + minSizes[index];
            if (!pushDivisions && index < offsets.Length - 1)
            {
                minOffset = Mathf.Max(minOffset, offsets[index + 1] - maxSizes[index + 1]);
            }
            if (newOffset < minOffset) newOffset = minOffset;
        }

        float actualDelta = newOffset - offsets[index];
        offsets[index] = newOffset;
        if (pushDivisions)
        {
            PushFrom(index + 1, actualDelta);
        }
        //Validate(index);//TODO TODO TODO TODO WRONG!?! validate change before pushing?
    }

    //	public void Validate(int index){//TODO implement max size
    //		float minOffset, maxOffset;
    //		if (index == 0){
    //			minOffset = Mathf.Max(minSizes[index], offsets[index+1]-maxSizes[index+1]);
    //			maxOffset = Mathf.Min(maxSizes[index], offsets[index+1]-minSizes[index+1]);
    //		}
    //		else if (index == offsets.Length-1) {
    //			minOffset = offsets[index-1] + minSizes[index];
    //			maxOffset = offsets[index-1] + maxSizes[index];
    //		}
    //		else {
    //			minOffset = Mathf.Max(offsets[index-1] + minSizes[index], offsets[index+1]-maxSizes[index+1]);
    //			maxOffset = Mathf.Min(offsets[index-1] + maxSizes[index], offsets[index+1]-minSizes[index+1]);
    //		}
    //		offsets[index] = Mathf.Clamp(offsets[index], minOffset, maxOffset);
    //	}


    public int Count
    {
        get { return offsets.Length; }
    }

    #region IEnumerable implementation

    public IEnumerator<float> GetEnumerator()
    {
        int index = 0;
        while (index < offsets.Length)
        {
            yield return offsets[index];
            index++;
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        int index = 0;
        while (index < offsets.Length)
        {
            yield return offsets[index];
            index++;
        }
    }

    #endregion

    public IEnumerable<Rect> HorizontalLayoutRects(Rect area)
    {
        yield return new Rect(area.x, area.y, offsets[0], area.height);
        for (int i = 1; i < offsets.Length; i++)
        {
            yield return new Rect(area.x + offsets[i - 1], area.y, offsets[i] - offsets[i - 1], area.height);
        }
    }

    public Rect GetHorizontalLayoutRect(int index, Rect area)
    {
        if (index == 0) return new Rect(area.x, area.y, offsets[0], area.height);
        else return new Rect(area.x + offsets[index - 1], area.y, offsets[index] - offsets[index - 1], area.height);
    }

    public IEnumerable<Rect> VerticalLayoutRects(Rect area)
    {
        yield return new Rect(area.x, area.y, area.width, offsets[0]);
        for (int i = 1; i < offsets.Length; i++)
        {
            yield return new Rect(area.x, area.y + offsets[i - 1], area.width, offsets[i] - offsets[i - 1]);
        }
    }

    public Rect GetVerticalLayoutRect(int index, Rect area)
    {
        if (index == 0) return new Rect(area.x, area.y, area.width, offsets[0]);
        else return new Rect(area.x, area.y + offsets[index - 1], area.width, offsets[index] - offsets[index - 1]);
    }

    //	private bool dragging;
    //	private int sliderDragged;
    //	public void DoHorizontalSliders(Rect area){
    //		Event e = Event.current;
    //
    //		if (autoExtend && e.type == EventType.Repaint) {
    //			if (offsets[offsets.Length-1] < area.width) offsets[offsets.Length-1] = area.width;
    ////			Validate(offsets.Length-1);//Recursive validate?
    //		}
    //
    //		float halfSliderSize = sliderSize / 2f;
    //		float baseOffset = area.x - halfSliderSize;
    //
    //		Rect sliderRect = new Rect(area);
    //		sliderRect.width = sliderSize;
    //		if (!dragging) {
    //			for (int i = 0; i < offsets.Length - 1; i++) {
    //				sliderRect.x = offsets[i] + baseOffset;
    //
    ////				GUI.DrawTexture(sliderRect, Texture2D.whiteTexture);
    //				EditorGUIUtility.AddCursorRect(sliderRect, MouseCursor.SplitResizeLeftRight);
    //				if (area.Overlaps(sliderRect) && sliderRect.Contains(e.mousePosition)){
    //					
    //					Debug.Log("Contains? "+sliderRect+" "+e.mousePosition);
    //					if (e.type == EventType.MouseDown) {
    //						dragging = true;
    //						sliderDragged = i;
    //						e.Use();
    //						Debug.Log("Dragging "+i);
    //					}
    //				}
    //			}
    //		}
    //		else {
    //			sliderRect.x = offsets[sliderDragged] - baseOffset;
    //			EditorGUIUtility.AddCursorRect(area, MouseCursor.SplitResizeLeftRight);
    //			if (e.type == EventType.MouseDrag) {
    //				Slide(sliderDragged, e.mousePosition.x - area.x - offsets[sliderDragged]);
    //				e.Use();
    //			}
    //			else if (e.type == EventType.MouseUp) {
    //				dragging = false;
    //				e.Use();
    //			}
    //		}
    //	}

    public void DoHorizontalSliders(Rect area)
    {
        for (int i = 0; i < offsets.Length - 1; i++)
        {
            float newOffset = HorizontalSlider(area, offsets[i] + area.x, sliderSize) - area.x;
            if (newOffset != offsets[i])
            {
                Slide(i, newOffset - offsets[i]);
            }
        }
    }

    public void DoVerticalSliders(Rect area)
    {
        for (int i = 0; i < offsets.Length - 1; i++)
        {
            float newOffset = VerticalSlider(area, offsets[i] + area.y, sliderSize) - area.y;
            if (newOffset != offsets[i])
            {
                Slide(i, newOffset - offsets[i]);
            }
        }
    }


    static bool draggingSlider;
    static int draggedControl;
    static public float HorizontalSlider(Rect area, float value, float size, GUIStyle style = null)
    {
        Event e = Event.current;
        int id = GUIUtility.GetControlID(FocusType.Passive);
        Rect sliderArea = new Rect(value - size / 2f, area.y, size, area.height);
        bool dragOn = draggingSlider && id == draggedControl;
        bool mouseInside = sliderArea.Contains(e.mousePosition);
        if (e.type == EventType.Repaint)
        {
            if (GUI.enabled) EditorGUIUtility.AddCursorRect(sliderArea, MouseCursor.SplitResizeLeftRight);
            if (style != null) style.Draw(sliderArea, false, GUI.enabled, dragOn, false);
        }
        else if (GUI.enabled)
        {
            if (e.type == EventType.ignore)
            {
                draggingSlider = false;
            }
            if (e.type == EventType.MouseDown)
            {
                if (mouseInside)
                {
                    draggingSlider = true;
                    GUIUtility.hotControl = draggedControl = id;
                    if (EditorWindow.focusedWindow != null) EditorWindow.focusedWindow.Repaint();
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseDrag)
            {
                if (dragOn)
                {
                    value = Mathf.Clamp(e.mousePosition.x, area.xMin, area.xMax);
                    if (EditorWindow.focusedWindow != null) EditorWindow.focusedWindow.Repaint();
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseUp)
            {
                if (dragOn)
                {
                    draggingSlider = false;
                    GUIUtility.hotControl = draggedControl = 0;
                    if (EditorWindow.focusedWindow != null) EditorWindow.focusedWindow.Repaint();
                    e.Use();
                }
            }
        }
        return value;
    }
    //Layout version?

    static public float VerticalSlider(Rect area, float value, float size, GUIStyle style = null)
    {
        Event e = Event.current;
        int id = GUIUtility.GetControlID(FocusType.Passive);
        Rect sliderArea = new Rect(area.x, value - size / 2f, area.width, size);
        bool dragOn = draggingSlider && id == draggedControl;
        bool mouseInside = sliderArea.Contains(e.mousePosition);
        if (e.type == EventType.Repaint)
        {
            if (GUI.enabled) EditorGUIUtility.AddCursorRect(sliderArea, MouseCursor.SplitResizeUpDown);
            if (style != null) style.Draw(sliderArea, false, GUI.enabled, dragOn, false);
        }
        else if (GUI.enabled)
        {
            if (e.type == EventType.MouseDown)
            {
                if (mouseInside)
                {
                    draggingSlider = true;
                    GUIUtility.hotControl = draggedControl = id;
                    if (EditorWindow.focusedWindow != null) EditorWindow.focusedWindow.Repaint();
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseDrag)
            {
                if (dragOn)
                {
                    value = Mathf.Clamp(e.mousePosition.y, area.yMin, area.yMax);
                    if (EditorWindow.focusedWindow != null) EditorWindow.focusedWindow.Repaint();
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseUp)
            {
                if (dragOn)
                {
                    draggingSlider = false;
                    GUIUtility.hotControl = draggedControl = 0;
                    if (EditorWindow.focusedWindow != null) EditorWindow.focusedWindow.Repaint();
                    e.Use();
                }
            }
        }
        return value;
    }

    public void DrawSlider(Rect area)
    {

    }

    public override string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder("[DivisionSlider: ", 15 + offsets.Length * 6);
        int i = 0;
        sb.Append(offsets[i].ToString("0"));
        while (++i < offsets.Length)
        {
            sb.Append(", " + offsets[i].ToString("0"));
        }
        return sb.Append(']').ToString();
    }
}