using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ڽ� ��� �� Success�� Running ���¸� ���� ��尡 �߻��� �� ������ �����ϰ� ����
/// </summary>
public class SelecterNode : INode
{
    List<INode> _childs;

    public SelecterNode(List<INode> childs)
    {
        _childs = childs;
    }

    public INode.ENodeState Evaluate()
    {
        if(_childs == null)
            return INode.ENodeState.Failure;

        foreach(var child in _childs)
        {
            switch(child.Evaluate())
            {
                case INode.ENodeState.Running:
                    return INode.ENodeState.Running;
                case INode.ENodeState.Success:
                    return INode.ENodeState.Success;
            }
        }

        return INode.ENodeState.Failure;
    }
}
