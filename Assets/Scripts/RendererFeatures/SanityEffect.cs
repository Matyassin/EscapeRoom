using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class GhostingFeature : ScriptableRendererFeature
{
    public Shader SanityShader;
    [Range(0f, 0.99f)] public float BlendAmount = 0.8f;
    public RenderPassEvent PassEvent = RenderPassEvent.AfterRenderingPostProcessing;

    private GhostingPass m_CustomPass;
    private Material m_RuntimeMat;

    public static float RuntimeBlendAmount = 0.0f;

    class GhostingPass : ScriptableRenderPass
    {
        private Material m_Material;
        private RTHandle m_HistoryHandle;

        public GhostingPass(Material mat, RenderPassEvent passEvent)
        {
            m_Material = mat;
            renderPassEvent = passEvent;
        }

        private class BlendPassData
        {
            public Material material;
            public float blendAmount;
            public TextureHandle sourceTex;
            public TextureHandle historyTex;
        }

        private class CopyPassData
        {
            public TextureHandle sourceTex;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (m_Material == null)
                return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            if (cameraData.cameraType != CameraType.Game)
                return;

            TextureHandle cameraColor = resourceData.cameraColor;
            if (!cameraColor.IsValid())
                return;

            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            RenderingUtils.ReAllocateHandleIfNeeded(ref m_HistoryHandle, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_HistoryTex");

            TextureHandle historyTexture = renderGraph.ImportTexture(m_HistoryHandle);
            TextureHandle tempTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_TempTex", false);

            using (var builder = renderGraph.AddRasterRenderPass<BlendPassData>("Ghosting Blend Pass", out var passData))
            {
                passData.material = m_Material;
                passData.blendAmount = GhostingFeature.RuntimeBlendAmount;
                passData.sourceTex = cameraColor;
                passData.historyTex = historyTexture;

                builder.UseTexture(passData.sourceTex, AccessFlags.Read);
                builder.UseTexture(passData.historyTex, AccessFlags.Read);
                builder.SetRenderAttachment(tempTexture, 0, AccessFlags.Write);

                builder.SetRenderFunc((BlendPassData data, RasterGraphContext context) =>
                {
                    data.material.SetFloat("_BlendAmount", data.blendAmount);
                    data.material.SetTexture("_HistoryTex", data.historyTex);
                    Blitter.BlitTexture(context.cmd, data.sourceTex, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<CopyPassData>("Ghosting Copy to Camera", out var passData))
            {
                passData.sourceTex = tempTexture;
                builder.UseTexture(passData.sourceTex, AccessFlags.Read);
                builder.SetRenderAttachment(cameraColor, 0, AccessFlags.Write);
                builder.SetRenderFunc((CopyPassData data, RasterGraphContext context) => { Blitter.BlitTexture(context.cmd, data.sourceTex, new Vector4(1, 1, 0, 0), 0, false); });
            }

            using (var builder = renderGraph.AddRasterRenderPass<CopyPassData>("Ghosting Copy to History", out var passData))
            {
                passData.sourceTex = tempTexture;
                builder.UseTexture(passData.sourceTex, AccessFlags.Read);
                builder.SetRenderAttachment(historyTexture, 0, AccessFlags.Write);
                builder.SetRenderFunc((CopyPassData data, RasterGraphContext context) => { Blitter.BlitTexture(context.cmd, data.sourceTex, new Vector4(1, 1, 0, 0), 0, false); });
            }
        }

        public void Dispose()
        {
            m_HistoryHandle?.Release();
        }
    }

    public override void Create()
    {
        if (SanityShader != null)
        {
            if (m_RuntimeMat != null) CoreUtils.Destroy(m_RuntimeMat);
            m_RuntimeMat = new Material(SanityShader);
            m_RuntimeMat.hideFlags = HideFlags.HideAndDontSave;
        }

        if (!Application.isPlaying)
        {
            RuntimeBlendAmount = BlendAmount;
        }

        m_CustomPass = new GhostingPass(m_RuntimeMat, PassEvent);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (m_RuntimeMat != null)
            renderer.EnqueuePass(m_CustomPass);
    }

    protected override void Dispose(bool disposing)
    {
        m_CustomPass?.Dispose();

        if (m_RuntimeMat != null)
        {
            CoreUtils.Destroy(m_RuntimeMat); m_RuntimeMat = null;
        }
    }
}
