using UnityEngine;
using Utils;

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
        }

        private void OnDestroy() => _renderTexture.Release();

        private void Update()
        {
            if (Screen.width != _renderTexture.width || Screen.height != _renderTexture.height)
            {
                _renderTexture.Release();
                _renderCamera.targetTexture = _renderTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
                _renderTexture.Create();
            }

            _renderSurface.material.SetTexture("_MainTex", _renderTexture);

            var worldInOtherPortal = WorldInOtherPortal(_playerCamera);

            _renderCamera.transform.FromMatrix(worldInOtherPortal);

            var p = new Plane(-_otherPortal.transform.forward, _otherPortal.transform.position);
            var clipPlaneWorldSpace = new Vector4(p.normal.x, p.normal.y, p.normal.z, p.distance);
            var clipPlaneCameraSpace =
                Matrix4x4.Transpose(Matrix4x4.Inverse(_renderCamera.worldToCameraMatrix)) * clipPlaneWorldSpace;

            var newMatrix = _renderCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
            _renderCamera.projectionMatrix = newMatrix;
        }

        private Matrix4x4 WorldInOtherPortal(Transform currentWorld)
        {
            var playerCameraWorld = currentWorld.localToWorldMatrix;
            var playerCameraInPortal = transform.worldToLocalMatrix * playerCameraWorld;
            var localReflected = Matrix4x4.Rotate(Quaternion.Euler(0.0f, 180.0f, 0.0f)) * playerCameraInPortal;
            return _otherPortal.transform.localToWorldMatrix * localReflected;
        }
    }
}