public interface INode
{
    // ��� ����
    public enum ENodeState
    {
        Running,
        Success,
        Failure
    }

    // ��� ���� ��ȯ
    public ENodeState Evaluate();
}