using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestGiverUI : MonoBehaviour
{
    public TMP_Text text;
    public TMP_InputField input;
    public Button buttonYes;
    public Button buttonNo;

    public AnimationCurve animShimmer;
    public float fAnimSpeed = 2f;
    public Image imageShimmer;

    private Color colorShimmer;

    void Start()
    {
        colorShimmer = imageShimmer.color;
    }

    void Update()
    {
        colorShimmer.a = animShimmer.Evaluate(Time.time * fAnimSpeed);
        imageShimmer.color = colorShimmer;
    }

}
