using System;
using UnityEngine;
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
        
        private UnityEngine.Camera _renderCamera;
        private RenderTexture      _renderTexture;
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

            var renderCameraObject = new GameObject(transform.name + " Camera");
            _renderCamera = renderCameraObject.AddComponent<UnityEngine.Camera>();
            _renderCamera.targetTexture = _renderTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
            _renderTexture.Create();

            if (VRHelper.Instance.VRMode)
                InitVRCamera();
        }

        private void InitVRCamera()
        {
            Valve.VR.EVREye eye;
            //if (isLeftEye)
            //{
                eye = Valve.VR.EVREye.Eye_Left;
            //}
            //else
            //{
            //    eye = Valve.VR.EVREye.Eye_Right;
            //}
            _renderCamera.projectionMatrix = HMDMatrix4x4ToMatrix4x4(SteamVR.instance.hmd.GetProjectionMatrix(eye, _renderCamera.nearClipPlane, _renderCamera.farClipPlane));
        }
        private void OnDestroy() => _renderTexture.Release();

        private void Update()
        {
            var targetWidth = Screen.width;
            var targetHeight = Screen.height;

            if (VRHelper.Instance.VRMode)
            {
                targetWidth  = UnityEngine.XR.XRSettings.eyeTextureWidth;
                targetHeight = UnityEngine.XR.XRSettings.eyeTextureHeight;
            }

            if (targetWidth != _renderTexture.width || targetHeight != _renderTexture.height)
            {
                _renderTexture.Release();
                _renderCamera.targetTexture = _renderTexture = new RenderTexture(targetWidth, targetHeight, 16, RenderTextureFormat.ARGB32);
                _renderTexture.Create();
            }

            _renderSurface.material.SetTexture("_MainTex", _renderTexture);

            var worldInOtherPortal = WorldInOtherPortal(_playerCamera);
            _renderCamera.transform.FromMatrix(worldInOtherPortal);

            if (VRHelper.Instance.VRMode)
            {
                Vector3 eyeOffset = Vector3.zero;
                //if (isLeftEye)
                //{
                    eyeOffset = SteamVR.instance.eyes[0].pos;
                //}
                //else
                //{
                //    eyeOffset = SteamVR.instance.eyes[1].pos;
                //}

                _renderCamera.transform.position += _renderCamera.transform.TransformVector(eyeOffset);
                _renderCamera.transform.localRotation = _renderCamera.transform.localRotation;
            }

            var p = new Plane(-_otherPortal.transform.forward, _otherPortal.transform.position);
            var clipPlaneWorldSpace = new Vector4(p.normal.x, p.normal.y, p.normal.z, p.distance);
            var clipPlaneCameraSpace =
                Matrix4x4.Transpose(Matrix4x4.Inverse(_renderCamera.worldToCameraMatrix)) * clipPlaneWorldSpace;
            
            _renderCamera.projectionMatrix = CalculateObliqueMatrix(_renderCamera.projectionMatrix, clipPlaneCameraSpace);
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