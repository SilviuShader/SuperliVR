using System;
using UnityEngine;
using UnityEngine.Rendering;
using Utils;
using Valve.VR;

namespace SuperliVR.Portals
{
    public class Portal : MonoBehaviour
    {
        [SerializeField]
        private Portal             _otherPortal;
        [SerializeField]
        private MeshRenderer       _renderSurface;
        [SerializeField]
        private Transform          _playerCamera;
        [SerializeField]
        private Material           _portalMaterial;
        [SerializeField]
        private Material           _vrPortalMaterial;
        
        private UnityEngine.Camera _renderCamera1;
        private UnityEngine.Camera _renderCamera2;

        private RenderTexture      _renderTexture1;
        private RenderTexture      _renderTexture2;
        private PortalBehaviour    _portalBehaviour;

        public void Teleport(Collider other)
        {
            var otherTransform = other.transform;
            var initialScale = otherTransform.localScale;

            otherTransform.FromMatrix(WorldInOtherPortal(otherTransform));
            
            var rigidbody = otherTransform.GetComponent<Rigidbody>();

            if (rigidbody != null)
            {
                rigidbody.position = otherTransform.position;
                rigidbody.rotation = otherTransform.rotation;
            }

            var scaleFactor = _otherPortal.transform.lossyScale.y / transform.lossyScale.y;
            otherTransform.localScale = initialScale * scaleFactor;
        }

        private void Awake()
        {
            _portalBehaviour = GetComponentInChildren<PortalBehaviour>();

            var renderCamera1Object = new GameObject(transform.name + " Left Camera");
            _renderCamera1 = renderCamera1Object.AddComponent<UnityEngine.Camera>();

            var renderCamera2Object = new GameObject(transform.name + " Right Camera");
            _renderCamera2 = renderCamera2Object.AddComponent<UnityEngine.Camera>();

            RecreateRenderTextures();

            if (VRHelper.Instance.VRMode)
                InitVRCameras();

            RenderPipelineManager.beginCameraRendering += OnRenderCallback;
        }

        private void OnDestroy()
        {
            RenderPipelineManager.beginCameraRendering -= OnRenderCallback;

            _renderTexture1.Release();
            _renderTexture2.Release();
        }

        private void Update() => RecreateRenderTextures();
        
        private void OnRenderCallback(ScriptableRenderContext context, UnityEngine.Camera cam)
        {
            if (cam != _renderCamera1 && cam != _renderCamera2)
                return;

            var worldInOtherPortal = WorldInOtherPortal(_playerCamera);
            _renderCamera1.transform.FromMatrix(worldInOtherPortal);
            _renderCamera2.transform.FromMatrix(worldInOtherPortal);

            var p = new Plane(-_otherPortal.transform.forward, _otherPortal.transform.position);
            var clipPlaneWorldSpace = new Vector4(p.normal.x, p.normal.y, p.normal.z, p.distance);

            if (!VRHelper.Instance.VRMode)
            {
                _renderSurface.material = _portalMaterial;
                _renderSurface.material.SetTexture("_MainTex", _renderTexture1);

                var clipPlaneCameraSpace =
                    Matrix4x4.Transpose(Matrix4x4.Inverse(_renderCamera1.worldToCameraMatrix)) * clipPlaneWorldSpace;

                _renderCamera1.projectionMatrix = _renderCamera1.CalculateObliqueMatrix(clipPlaneCameraSpace);
            }
            else
            {
                _renderSurface.material = _vrPortalMaterial;
                _renderSurface.material.SetTexture("_LeftEyeTex", _renderTexture1);
                _renderSurface.material.SetTexture("_RightEyeTex", _renderTexture2);

                _renderCamera1.transform.position += _renderCamera1.transform.TransformVector(SteamVR.instance.eyes[0].pos);
                _renderCamera2.transform.position += _renderCamera2.transform.TransformVector(SteamVR.instance.eyes[1].pos);

                var clipPlaneCamera1Space =
                    Matrix4x4.Transpose(Matrix4x4.Inverse(_renderCamera1.worldToCameraMatrix)) * clipPlaneWorldSpace;
                var clipPlaneCamera2Space =
                    Matrix4x4.Transpose(Matrix4x4.Inverse(_renderCamera2.worldToCameraMatrix)) * clipPlaneWorldSpace;

                _renderCamera1.projectionMatrix = CalculateObliqueMatrix(_renderCamera1.projectionMatrix, clipPlaneCamera1Space);
                _renderCamera2.projectionMatrix = CalculateObliqueMatrix(_renderCamera2.projectionMatrix, clipPlaneCamera2Space);
            }

        }

        private void RecreateRenderTextures()
        {
            var targetWidth = Screen.width;
            var targetHeight = Screen.height;

            if (VRHelper.Instance.VRMode)
            {
                targetWidth  = UnityEngine.XR.XRSettings.eyeTextureWidth;
                targetHeight = UnityEngine.XR.XRSettings.eyeTextureHeight;
            }

            var currentWidth = _renderTexture1  != null ? _renderTexture1.width  : 0;
            var currentHeight = _renderTexture1 != null ? _renderTexture1.height : 0;

            if (targetWidth != currentWidth || targetHeight != currentHeight)
            {
                if (_renderTexture1 != null)
                    _renderTexture1.Release();

                if (_renderTexture2 != null)
                    _renderTexture2.Release();

                _renderCamera1.targetTexture = _renderTexture1 = new RenderTexture(targetWidth, targetHeight, 16, RenderTextureFormat.ARGB32);
                _renderTexture1.Create();

                _renderCamera2.targetTexture = _renderTexture2 = new RenderTexture(targetWidth, targetHeight, 16, RenderTextureFormat.ARGB32);
                _renderTexture2.Create();
            }
        }
        private void InitVRCameras()
        {
            _renderCamera1.projectionMatrix = HMDMatrix4x4ToMatrix4x4(SteamVR.instance.hmd.GetProjectionMatrix(EVREye.Eye_Left,  _renderCamera1.nearClipPlane, _renderCamera1.farClipPlane));
            _renderCamera2.projectionMatrix = HMDMatrix4x4ToMatrix4x4(SteamVR.instance.hmd.GetProjectionMatrix(EVREye.Eye_Right, _renderCamera1.nearClipPlane, _renderCamera1.farClipPlane));
        }

        private Matrix4x4 WorldInOtherPortal(Transform currentWorld)
        {
            var playerCameraWorld = currentWorld.localToWorldMatrix;
            var playerCameraInPortal = transform.worldToLocalMatrix * playerCameraWorld;
            var localReflected = Matrix4x4.Rotate(Quaternion.Euler(0.0f, 180.0f, 0.0f)) * playerCameraInPortal;
            return _otherPortal.transform.localToWorldMatrix * localReflected;
        }

        private Matrix4x4 HMDMatrix4x4ToMatrix4x4(HmdMatrix44_t input)
        {
            var m = Matrix4x4.identity;

            m[0, 0] = input.m0;
            m[0, 1] = input.m1;
            m[0, 2] = input.m2;
            m[0, 3] = input.m3;

            m[1, 0] = input.m4;
            m[1, 1] = input.m5;
            m[1, 2] = input.m6;
            m[1, 3] = input.m7;

            m[2, 0] = input.m8;
            m[2, 1] = input.m9;
            m[2, 2] = input.m10;
            m[2, 3] = input.m11;

            m[3, 0] = input.m12;
            m[3, 1] = input.m13;
            m[3, 2] = input.m14;
            m[3, 3] = input.m15;

            return m;
        }

        private static Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane)
        {
            Matrix4x4 obliqueMatrix = projection;
            Vector4 q = projection.inverse * new Vector4(
                Math.Sign(clipPlane.x),
                Math.Sign(clipPlane.y),
                1.0f,
                1.0f
            );
            Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));
            obliqueMatrix[2] = c.x - projection[3];
            obliqueMatrix[6] = c.y - projection[7];
            obliqueMatrix[10] = c.z - projection[11];
            obliqueMatrix[14] = c.w - projection[15];
            return obliqueMatrix;
        }
    }
}