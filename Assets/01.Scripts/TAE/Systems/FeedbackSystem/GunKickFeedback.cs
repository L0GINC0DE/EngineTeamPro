using UnityEngine;

namespace _01.Scripts.KTH.FeedbackSystem
{
    public class GunKickFeedback : AbstractFeedback
    {
        
        [SerializeField] private Transform gunTransform;
        [SerializeField] private float gunRecovery = 12f;
        [SerializeField] private float kickDistance = 0.1f;
        [SerializeField] private float kickAngle = 5f;
        private Vector3 _originalLocalPosition;
        private float _originalLocalAngleZ; //초기 z축회전과 위치를 저장한다. 

        private void Awake()
        {
            _originalLocalPosition = gunTransform.localPosition;
            _originalLocalAngleZ = gunTransform.localEulerAngles.z;
        }

        private void Update()
        {
            gunTransform.localPosition = Vector3.Lerp(
                gunTransform.localPosition, _originalLocalPosition, gunRecovery * Time.deltaTime);

            float currentZ = gunTransform.localEulerAngles.z;
            float targetZ = Mathf.LerpAngle(currentZ, _originalLocalAngleZ, gunRecovery * Time.deltaTime);
            gunTransform.localEulerAngles = new Vector3(0, 0, targetZ);
        }

        public override void CreateFeedback()
        {
            float zAngle = gunTransform.eulerAngles.z; 
            
            //여기에 추가 작업해야해.

            gunTransform.localPosition -= new Vector3(kickDistance, 0, 0); //뒤쪽으로 백
            gunTransform.localEulerAngles += new Vector3(0, 0, kickAngle);
        }

        public override void FinishFeedback()
        {
        }
    }
}