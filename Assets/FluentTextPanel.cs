using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static UnityEngine.UI.ContentSizeFitter;

public class FluentTextPanel : MonoBehaviour
{
    //更改的字体目标
    public Text TargetText;
    //隐藏的text组件，只用于确定panel需要变换的大小，后期改为自动生成
    public Text HiddenText;
    public string[] Words;
    private int wordIndex = 0;
    public float MaxWidth = 400;
    public float ExtraWidth = 30;
    public float ExtraHeight = 30;
    private Vector2 TargetSize
    {
        get { return GetComponent<RectTransform>().sizeDelta; }
        set { GetComponent<RectTransform>().sizeDelta = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ShowNextWord();
        }
    }

    private void ShowNextWord()
    {
        StartCoroutine(StartChangeWord(Words[wordIndex]));
        wordIndex = (wordIndex + 1) % (Words.Length);
    }

    /// <summary>
    /// 传入所需要显示的话并更新panel大小
    /// </summary>
    /// <param name="changeWord"></param>
    /// <returns></returns>
    IEnumerator StartChangeWord(string Word)
    {
        string changeWord = Word.Replace("\\n", "\n");
        TargetText.text = "";
        HiddenText.text = changeWord;
        RefreshSize();
        yield return new WaitForSeconds(0.3f);
        float textTime = changeWord.Length * 0.08f;
        TargetText.DOText(changeWord, textTime).SetEase(Ease.Linear);
    }

    //刷新文字panel以及文字本身大小
    public void RefreshSize()
    {
        var textPanelSize = GetPreferredSize(HiddenText.gameObject);
        //隐藏panel的原大小
        var rawPanelSize = textPanelSize;

        //增加边框余地
        textPanelSize += new Vector2(ExtraWidth, ExtraHeight);
        //增加随机大小
        textPanelSize += new Vector2(Random.Range(0, 50), Random.Range(0, 50));
        //刷新显示文字大小
        TargetText.GetComponent<RectTransform>().sizeDelta = rawPanelSize;
        //让文字外框做一个变形动画
        DOTween.To(() => TargetSize, x => TargetSize = x, textPanelSize, 0.3f);

    }

    //立即获取ContentSizeFitter的区域
    public Vector2 GetPreferredSize(GameObject obj)
    {
        //刷新网格
        LayoutRebuilder.ForceRebuildLayoutImmediate(obj.GetComponent<RectTransform>());

        RefreshPanelSize(obj);

        return new Vector2(HandleSelfFittingAlongAxis(0, obj), HandleSelfFittingAlongAxis(1, obj));
    }
    
    //获取隐藏panel的宽或高
    private float HandleSelfFittingAlongAxis(int axis, GameObject obj)
    {
        //获取当前隐藏panel选择的适配模式
        FitMode fitting = (axis == 0 ? obj.GetComponent<ContentSizeFitter>().horizontalFit : obj.GetComponent<ContentSizeFitter>().verticalFit);
        if (fitting == FitMode.MinSize)
        {
            return LayoutUtility.GetMinSize(obj.GetComponent<RectTransform>(), axis);
        }
        else if (fitting == FitMode.PreferredSize)
        {
            return LayoutUtility.GetPreferredSize(obj.GetComponent<RectTransform>(), axis);
        }
        else
        {
            //如果是unconstrained的话直接获取当前的大小
            return axis == 0 ? obj.GetComponent<RectTransform>().sizeDelta.x : obj.GetComponent<RectTransform>().sizeDelta.y;
        }
    }

    //刷新隐藏文字和显示文字的宽，如果偏大就设置为最大值，如果偏小就缩小为合适的宽度
    private void RefreshPanelSize(GameObject obj)
    {
        if (LayoutUtility.GetPreferredSize(obj.GetComponent<RectTransform>(), 0) < MaxWidth)
        {
            var origin = obj.GetComponent<RectTransform>().sizeDelta;
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(LayoutUtility.GetPreferredSize(obj.GetComponent<RectTransform>(), 0), origin.y);
            TargetText.GetComponent<RectTransform>().sizeDelta = new Vector2(LayoutUtility.GetPreferredSize(obj.GetComponent<RectTransform>(), 0), origin.y);
        }
        else
        {
            var origin = obj.GetComponent<RectTransform>().sizeDelta;
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(MaxWidth, origin.y);
            TargetText.GetComponent<RectTransform>().sizeDelta = new Vector2(MaxWidth, origin.y);
        }
    }
}
