using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TBallista : StructureBase
{
    [SerializeField] private bool isUsing = false;

    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Collider[] enemyColliders;
    [SerializeField] private float attackRange;


    // TODO: ����, ScriptableObject�� hp�� ���� �ʿ���~~~
    private int currentHP = 80;


    #region �⺻
    public override void Init()
    {
        
    }

    void Update()
    {
        
    }
    #endregion 

    void Use()
    {
        if (isUsing) return;
        isUsing = true;
    }

    bool Detect()
    {
        enemyColliders = Physics.OverlapSphere(transform.position, attackRange, enemyLayer, QueryTriggerInteraction.Ignore);
        return true;
    }

    void LookAt()
    {
        
    }
    
    void Fire()
    {

    }
}
