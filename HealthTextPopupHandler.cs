using StdNounou;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[RequireComponent(typeof(HealthSystem))]
public class HealthTextPopupHandler : MonoBehaviourEventsHandler
{
    [SerializeField] private HealthSystem targetSystem;
    [SerializeField] private Vector3 successiveOffset = new Vector3(0,1,0);

    [SerializeField] private SO_TextPopupData normalDamagesData;
    [SerializeField] private SO_TextPopupData criticalDamagesData;
    [SerializeField] private SO_TextPopupData healDamagesData;

    private List<TextPopup> textPopupList;

    private void Reset()
    {
#if UNITY_EDITOR
        targetSystem = this.GetComponent<HealthSystem>();
        normalDamagesData = ResourcesObjectLoader.GetTextPopupDataHolder().GetAsset("NormalDamages") as SO_TextPopupData;
        criticalDamagesData = ResourcesObjectLoader.GetTextPopupDataHolder().GetAsset("CriticalDamages") as SO_TextPopupData;
        healDamagesData = ResourcesObjectLoader.GetTextPopupDataHolder().GetAsset("HealDamages") as SO_TextPopupData;
        EditorUtility.SetDirty(this); 
#endif
    }

    protected override void EventsSubscriber()
    {
        targetSystem.OnTookDamages += OnTargetTookDamages;
        targetSystem.OnHealed += OnTargetHealed;
    }

    protected override void EventsUnSubscriber()
    {
        if (targetSystem != null)
        {
            targetSystem.OnTookDamages -= OnTargetTookDamages;
            targetSystem.OnHealed -= OnTargetHealed;
        }

    }

    protected override void Awake()
    {
        base.Awake();
        textPopupList = new List<TextPopup>();
    }

    private void OnTargetTookDamages(IDamageable.DamagesData damageData)
    {
        CreateText(text: damageData.Damages.ToString(),
                   data: damageData.IsCrit ? criticalDamagesData : normalDamagesData);

    }

    private void OnTargetHealed(float healAmount)
    {
        CreateText(text: healAmount.ToString(),
                   data: healDamagesData);
    }

    private void CreateText(string text, SO_TextPopupData data)
    {
        TextPopup current = TextPopup.Create(text, targetSystem.GetHealthPopupPosition(), data);
        current.OnEnd += OnTextEnded;
        for (int i = 0; i < textPopupList.Count; i++)
        {
            textPopupList[i].AddToTargetPosition(successiveOffset);
            textPopupList[i].SetListIndex(textPopupList.Count - i);
        }
        textPopupList.Add(current);
    }

    private void OnTextEnded(TextPopup text)
    {
        text.OnEnd -= OnTextEnded;
        textPopupList.Remove(text);
    }
}
