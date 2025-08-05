using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GrayOverlayPass : ScriptableRenderPass
{
    private Material material;
    private RenderTargetIdentifier source;
    private RenderTargetHandle tempTexture;

    public GrayOverlayPass(Material material)
    {
        this.material = material;
        tempTexture.Init("_TempRT");
        this.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public void Setup(RenderTargetIdentifier source)
    {
        this.source = source;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get("GrayOverlayPass");

        // �ӽ� ���� �ؽ�ó ����
        RenderTextureDescriptor cameraTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
        cmd.GetTemporaryRT(tempTexture.id, cameraTextureDesc);

        // �ҽ� �ؽ�ó�� �ӽ� �ؽ�ó�� ����
        Blit(cmd, source, tempTexture.Identifier(), material);

        // �ӽ� �ؽ�ó�� �ٽ� ī�޶� Ÿ������ ����
        Blit(cmd, tempTexture.Identifier(), source);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(tempTexture.id);
    }
}