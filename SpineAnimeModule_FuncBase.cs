using UnityEngine;
using UnityEngine.Events;
using Spine.Unity;
using Spine;
using EventData = Spine.EventData;
using Event = Spine.Event;
using Animation = Spine.Animation;
using lLCroweTool.Dictionary;
//using lLCroweTool.LogSystem;

namespace lLCroweTool.AnimeSystem.Spine
{
    public abstract class SpineAnimeModule_FuncBase : MonoBehaviour
    {
        //CPU를 조질까
        //메모리를 조잘까
        public SkeletonAnimation skeletonAnimation;

        public bool isRightCurrent = false;

        public SpineAnimDefineInfoBook spineAnimDefineInfoBook = new SpineAnimDefineInfoBook();
        public SpineAttachmentInfoBook spineAttachmentInfoBook = new SpineAttachmentInfoBook();

        //리셋을 체크해봐야될듯
        public string currentMainAnimName;//현재 지정된 대상 => SpineAnimDefineInfo의 이름을 집어넣을것//루프 애님일시

        protected virtual void Awake()
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();
            skeletonAnimation.Initialize(false);

            //로그등록
            //LogManager.Register("SpineAnimeModuleLog", "SpineAnimeModuleLog.txt", false, true);
            //LogManager.Log("SpineAnimeModuleLog", gameObject.name + "AnimModulePart_Awaking");

            spineAnimDefineInfoBook.Init(this);
            spineAttachmentInfoBook.Init(this);
            InitSpineData();//데이터초기화
        }

        /// <summary>
        /// 정의된 스파인데이터를 초기화하는 구역
        /// </summary>
        public abstract void InitSpineData();

        //=========================================
        //데이터 정의관련
        //=========================================

        /// <summary>
        /// 스파인정보 기본기능들
        /// </summary>
        public interface ISpineDataInfo
        {
            /// <summary>
            /// 데이터초기화
            /// </summary>
            /// <param name="sAMF">스파인애님모듈</param>
            public void Init(SpineAnimeModule_FuncBase sAMF);
        }

        public enum AniPlayType
        {
            Set,
            Add,
            Emtpy,
        }

        /// <summary>
        /// SpineAnimDefineInfo를 여러개 가진 정보클래스
        /// </summary>
        [System.Serializable]
        public class SpineAnimDefineInfoBook : ISpineDataInfo
        {
            //SpineAnimDefineInfo을 한번더 래핑한 형태//집어넣고 비어버리기
            public SpineAnimDefineInfo[] spineAnimDefineInfoArray = new SpineAnimDefineInfo[0];

            private class SpineAnimDefineInfoBible : CustomDictionary<string, SpineAnimDefineInfo> { }//Enum처리할시 같이처리
            [HideInInspector]
            private SpineAnimDefineInfoBible spineAnimDefineInfoBible = new SpineAnimDefineInfoBible();

            public void Init(SpineAnimeModule_FuncBase sAMF)
            {
                for (int i = 0; i < spineAnimDefineInfoArray.Length; i++)
                {
                    SpineAnimDefineInfo temp = spineAnimDefineInfoArray[i];
                    temp.Init(sAMF);
                    spineAnimDefineInfoBible.Add(temp.animName, temp);
                }
            }

            /// <summary>
            /// 애님작동
            /// </summary>
            /// <param name="sAMF">스파인애님모듈</param>
            /// <param name="animID">애님ID</param>
            public void ActionAnim(SpineAnimeModule_FuncBase sAMF, string animID)
            {
                if (!spineAnimDefineInfoBible.ContainsKey(animID))
                {
                    return;
                }

                //동일한 애님작동시 넘김//지정은 SpineAnimData_Base에서
                if (animID == sAMF.currentMainAnimName)
                {
                    return;
                }

                //애님작동
                spineAnimDefineInfoBible[animID].ActionAnim(sAMF);
            }
        }

        /// <summary>
        /// 애님정의 정보
        /// </summary>
        [System.Serializable]
        public class SpineAnimDefineInfo : ISpineDataInfo
        {
            //여러애님을 동시에 작업하기 위한 클래스
            [Header("애님이름")]
            public string animName;//작동될 이름처리//언젠간 Enum으로 처리//Enum은 자동화가 필요해서 흠//유니크하게 처리해야됨
            [Header("기존까지 작동됫던 애님을 정리")]
            public bool isClearAnim = false;

            [Space]
            [Header("스파인애님 설정")]
            public SpineAnimData[] spineAnimSetDataArray = new SpineAnimData[0];
            [Header("스파인애님Ref 설정")]
            public SpineAnimRefData[] spineAnimRefDataArray = new SpineAnimRefData[0];

            public void Init(SpineAnimeModule_FuncBase sAMF)
            {
                for (int i = 0; i < spineAnimSetDataArray.Length; i++)
                {
                    SpineAnimData_Base temp = spineAnimSetDataArray[i];
                    temp.Init(sAMF);
                }
                for (int i = 0; i < spineAnimSetDataArray.Length; i++)
                {
                    SpineAnimData_Base temp = spineAnimSetDataArray[i];
                    temp.Init(sAMF);
                }
            }

            /// <summary>
            /// 지정된 애님작동
            /// </summary>
            /// <param name="sAMF">스파인애님모듈</param>
            public void ActionAnim(SpineAnimeModule_FuncBase sAMF)
            {
                if (isClearAnim)
                {
                    ClearAnimation(sAMF);
                }

                for (int i = 0; i < spineAnimSetDataArray.Length; i++)
                {
                    SpineAnimData_Base temp = spineAnimSetDataArray[i];
                    temp.ActionAnim(sAMF, animName);
                }
                for (int i = 0; i < spineAnimRefDataArray.Length; i++)
                {
                    SpineAnimData_Base temp = spineAnimRefDataArray[i];
                    temp.ActionAnim(sAMF, animName);
                }
            }
        }

        /// <summary>
        /// 스파인애님데이터_기초
        /// </summary>
        [System.Serializable]
        public abstract class SpineAnimData_Base : ISpineDataInfo
        {
            public bool loop;
            [TooltipAttribute("AddAnim일시 작동되는 값")]
            public float delay = 0.5f;
            public int trackNumber;//트랙관련//유니크해야됨
            public AniPlayType aniPlayType;//애님 작동방식

            /// <summary>
            /// 애님 작동
            /// </summary>
            /// <param name="sAMF">스파인애님모듈</param>
            /// <param name="name">애니메이션 이름</param>
            public virtual void ActionAnim(SpineAnimeModule_FuncBase sAMF, string name)
            {
                switch (aniPlayType)
                {
                    case AniPlayType.Set:
                        SetAnim(sAMF, loop);
                        break;
                    case AniPlayType.Add:
                        AddAnim(sAMF, loop, delay);
                        break;
                    case AniPlayType.Emtpy:
                        //현재는 0번트랙외에 Loop가 아닌 대상일때 마지막에 Empty애님으로 들어감
                        //애님기능처리할 예정이였는데 몰?루
                        break;
                }

                if (loop)
                {
                    //루프일시 메인 애님이름 지정
                    sAMF.currentMainAnimName = name;
                }
                else
                {
                    //메인트랙이 아니면 해당애니메이션트랙을 비어버리게 하기
                    if (trackNumber != 0)
                    {
                        sAMF.skeletonAnimation.AnimationState.AddEmptyAnimation(trackNumber, 0.3f, 0.2f);
                    }
                }
            }

            /// <summary>
            /// 애님셋
            /// </summary>
            /// <param name="sAMF">스파인애님모듈</param>
            /// <param name="loop">반복여부</param>
            public abstract void SetAnim(SpineAnimeModule_FuncBase sAMF, bool loop);

            /// <summary>
            /// 애님 큐에 추가
            /// </summary>
            /// <param name="sAMF">스파인애님모듈</param>
            /// <param name="loop">반복여부</param>
            /// <param name="delay">딜레이</param>
            public abstract void AddAnim(SpineAnimeModule_FuncBase sAMF, bool loop, float delay = 0.5f);

            public abstract void Init(SpineAnimeModule_FuncBase sAMF);//인터페이스
        }

        /// <summary>
        /// 스파인애님Ref 데이터(부속품)
        /// </summary>
        [System.Serializable]
        public class SpineAnimRefData : SpineAnimData_Base
        {
            public AnimationReferenceAsset spineAnimRefData;//타겟애님

            public override void Init(SpineAnimeModule_FuncBase sAMF)
            {
                if (ReferenceEquals(spineAnimRefData, null))
                {
                    //없으면
                    //로그찍기
                    //LogManager.Log("SpineAnimeModuleLog", "'" + sAMF.name + "'에 AnimRef가 존재하지 않습니다." + spineAnimRefData.name);                    
                }
            }

            public override void SetAnim(SpineAnimeModule_FuncBase sAMF, bool loop)
            {
                //현재애니메이션을 중지시키고 새롭게 세팅한 애니메이션을 재성//믹스(블랜드)데이터가 없으면 혼합이 안됨//믹스 == 블랜드
                //트랙, 애니메이션 데이터, 루프여부
                sAMF.skeletonAnimation.AnimationState.SetAnimation(trackNumber, spineAnimRefData, loop);
            }

            public override void AddAnim(SpineAnimeModule_FuncBase sAMF, bool loop, float delay = 0.5f)
            {
                //애니메이션 큐에 추가시켜서 애니메이션을 작동
                //delay가 0 이하이면 믹스데이터에 있는 믹스지속시간//초과이면 그대로 지연시간
                //루프일시 한바퀴돌고 작동
                //트랙, 애니메이션 데이터, 루프여부, delay
                sAMF.skeletonAnimation.AnimationState.AddAnimation(trackNumber, spineAnimRefData, loop, delay);
            }
        }

        /// <summary>
        /// 스파인애님 데이터(부속품)
        /// </summary>
        [System.Serializable]
        public class SpineAnimData : SpineAnimData_Base
        {
            [SpineAnimation]
            public string spineAnim;//스파인애님이름

            [Space]
            //캐싱용도
            public Animation spineAnimData;//타겟애님

            public override void Init(SpineAnimeModule_FuncBase sAMF)
            {
                spineAnimData = sAMF.skeletonAnimation.AnimationState.Data.SkeletonData.FindAnimation(spineAnim);
                if (ReferenceEquals(spineAnimData, null))
                {
                    //LogManager.Log("SpineAnimeModuleLog", "'" + sAMF.name + "'에 AnimData의 " + spineAnim + "이름이 할당되지 않습니다.");
                    return;
                }
            }

            public override void SetAnim(SpineAnimeModule_FuncBase sAMF, bool loop)
            {
                //현재애니메이션을 중지시키고 새롭게 세팅한 애니메이션을 재성//믹스(블랜드)데이터가 없으면 혼합이 안됨//믹스 == 블랜드
                //트랙, 애니메이션 데이터, 루프여부
                sAMF.skeletonAnimation.AnimationState.SetAnimation(trackNumber, spineAnimData, loop);
            }

            public override void AddAnim(SpineAnimeModule_FuncBase sAMF, bool loop, float delay = 0.5f)
            {
                //애니메이션 큐에 추가시켜서 애니메이션을 작동
                //delay가 0 이하이면 믹스데이터에 있는 믹스지속시간//초과이면 그대로 지연시간
                //루프일시 한바퀴돌고 작동
                //트랙, 애니메이션 데이터, 루프여부, delay
                sAMF.skeletonAnimation.AnimationState.AddAnimation(trackNumber, spineAnimData, loop, delay);
            }
        }

        /// <summary>
        /// SpineAttachmentInfo를 여러개 가진 정보클래스
        /// </summary>
        public class SpineAttachmentInfoBook : ISpineDataInfo
        {
            //스파인어태치먼트에 직접적으로
            public SpineAttachmentInfo[] spineAttachmentInfoArray = new SpineAttachmentInfo[0];
            private class SpineAttachmentInfoBible : CustomDictionary<string, SpineAttachmentInfo> { }//Enum처리할시 같이처리
            private SpineAttachmentInfoBible spineAttachmentInfoBible = new SpineAttachmentInfoBible();
            public void Init(SpineAnimeModule_FuncBase sAMF)
            {
                for (int i = 0; i < spineAttachmentInfoArray.Length; i++)
                {
                    SpineAttachmentInfo temp = spineAttachmentInfoArray[i];
                    temp.Init(sAMF);
                    spineAttachmentInfoBible.Add(temp.attachmentNameID, temp);
                }
            }

            /// <summary>
            /// 어태치먼트 변경
            /// </summary>
            /// <param name="attackmentNameID">어태치먼트ID</param>
            public void ActionAttackment(string attackmentNameID)
            {
                if (!spineAttachmentInfoBible.ContainsKey(attackmentNameID))
                {
                    return;
                }

                //어태치변경작동
                spineAttachmentInfoBible[attackmentNameID].SetThisAttachment();
            }
        }

        /// <summary>
        /// 스파인어태치먼트 데이터
        /// </summary>
        [System.Serializable]
        public class SpineAttachmentInfo : ISpineDataInfo
        {
            //세팅해줘야됨
            [SpineAttachment]
            public string attachmentNameID;//변경할 어태치//아이디대상
            [SpineSlot]
            public string slotName;//변경할 슬롯
            
            [Space]
            //캐싱용도
            public Attachment attachment;
            public Slot slot;
            public void Init(SpineAnimeModule_FuncBase sAMF)
            {
                if (string.IsNullOrEmpty(slotName) || string.IsNullOrEmpty(attachmentNameID))
                {
                    //LogManager.Log("SpineAnimeModuleLog", "'" + sAMF.name + "'의 " + slotName + " , " + attachmentNameID + "이 제대로 설정되지않아 비어있습니다.");
                    return;
                }

                slot = sAMF.skeletonAnimation.skeleton.FindSlot(slotName);
                attachment = sAMF.skeletonAnimation.skeleton.GetAttachment(slotName, attachmentNameID);
            }

            /// <summary>
            /// 지정된 어태치먼트를 세팅
            /// </summary>
            public void SetThisAttachment()
            {
                if (slot == null)
                {
                    return;
                }
                slot.Attachment = attachment;
            }
        }

        /// <summary>
        /// 스파인이벤트 정보
        /// </summary>
        [System.Serializable]
        public class SpineEventInfo : ISpineDataInfo
        {
            [SpineEvent]
            public string eventName;
            private EventData eventData;//지정된 이벤트
            [Space]
            public UnityEvent unityEvent = new UnityEvent();//작동될 이벤트

            public void Init(SpineAnimeModule_FuncBase sAMF)
            {
                eventData = sAMF.skeletonAnimation.Skeleton.Data.FindEvent(eventName);
                sAMF.skeletonAnimation.AnimationState.Event += UpdateStateEvent;
            }

            /// <summary>
            /// 애님상태 업데이트. 스파인에서 처리
            /// </summary>
            /// <param name="trackEntry">트랙진입점</param>
            /// <param name="e">이벤트</param>
            private void UpdateStateEvent(TrackEntry trackEntry, Event e)
            {
                if (eventData != e.Data)
                {
                    return;
                }
                unityEvent.Invoke();
            }

            /// <summary>
            /// 작동될 이벤트 추가(세팅)
            /// </summary>
            /// <param name="action">함수</param>
            public void AddUnityEvent(UnityAction action)
            {
                //AddListener(delegate{함수();})
                unityEvent.RemoveAllListeners();
                unityEvent.AddListener(action);
            }
        }

        /// <summary>
        /// 스파인스킨 정보
        /// </summary>
        [System.Serializable]
        public class SpineSkinInfo : ISpineDataInfo
        {
            //이 구역은 다른구역과 다르게 스킨이라는걸 각각들고 있는것보다
            //특정데이터에 맞는 스킨들을 한구역에서 가지고 잇는게 더좋아보여서 Static이 괜찮아보인다 생각됨.
            //20230207
            //나중에 전체적으로 데이터는 스크립터블쪽으로 이동시킬예정

            [System.Serializable]
            public class SpineSkinBible : CustomDictionary<string, Skin> { }
            public SpineSkinBible spineSkinBible = new SpineSkinBible();

            public void Init(SpineAnimeModule_FuncBase sAMF)
            {
                SkeletonData data = sAMF.skeletonAnimation.SkeletonDataAsset.GetAnimationStateData().SkeletonData;

                foreach (Skin skin in data.Skins)
                {
                    spineSkinBible.Add(skin.Name, skin);
                }
            }

            /// <summary>
            /// 지정된 스킨을 세팅하는 함수
            /// </summary>
            /// <param name="sAMF"></param>
            /// <param name="skinName"></param>
            public void SetSkin(SpineAnimeModule_FuncBase sAMF, string skinName)
            {
                //에디터, 런타임에서만 작동
                if (!spineSkinBible.ContainsKey(skinName))
                {
                    return;
                }
                sAMF.skeletonAnimation.skeleton.SetSkin(spineSkinBible[skinName]);
            }
        }

        //스파인 제약조건같은경우 종류가 있으면서 정형화 안되있어서
        //필요할때 처리하기 
        //public class SpineIkConstraintInfoData
        //{

        //}

        //=========================================
        //기능관련(스태딕)
        //=========================================

        /// <summary>
        /// 타겟본을 마우스위치에 맞게 움직이게하는 함수
        /// </summary>
        /// <param name="mousePosition">마우스위치</param>
        /// <param name="skeletonAnimation">스컬레톤애니메이션</param>
        /// <param name="bone">타겟본</param>
        public static void Aim(Vector3 mousePosition, SpineAnimeModule_FuncBase sAMF, Bone bone)
        {
            //에임관련
            Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            Vector3 skeletonSpacePoint = sAMF.skeletonAnimation.transform.InverseTransformPoint(worldMousePosition);
            SkeletonAnimation skeletonAnimation = sAMF.skeletonAnimation;
            skeletonSpacePoint.x *= skeletonAnimation.Skeleton.ScaleX;
            skeletonSpacePoint.y *= skeletonAnimation.Skeleton.ScaleY;
            bone.SetLocalPosition(skeletonSpacePoint);//IKBone
        }

        /// <summary>
        /// 타겟본을 마우스위치에 맞게 움직이게하는 함수
        /// </summary>
        /// <param name="skeletonAnimation">스컬레톤애니메이션</param>
        /// <param name="bone">타겟본</param>
        public static void Aim(SpineAnimeModule_FuncBase sAMF, Bone bone)
        {
            //에임관련
            Vector3 mousePosition = Input.mousePosition;
            Aim(mousePosition, sAMF, bone);
        }

        /// <summary>
        /// 스컬레톤애니메이션 애님 리셋
        /// </summary>
        /// <param name="anim">스컬레톤애니메이션</param>
        public static void ClearAnimation(SpineAnimeModule_FuncBase sAMF)
        {
            //sAMF.skeletonAnimation.ClearState();//GetComponent가 존재
            sAMF.skeletonAnimation.state.ClearTracks();//따로 호출해주는게 좋아보임

            //클리어할시 바이블도 정리(제작대기)


        }

        //=========================================
        //기능관련
        //=========================================


        /// <summary>
        /// 뒤집기 스케일로 해준다
        /// </summary>
        private void FlipScale()
        {
            isRightCurrent = !isRightCurrent;
            skeletonAnimation.Skeleton.ScaleX *= -1;
        }

        /// <summary>
        /// 뒤집기 스프라이트랜더러의 플립을 이용한다.
        /// </summary>
        private void FlipSpriteRenderer()
        {
            isRightCurrent = !isRightCurrent;
            skeletonAnimation.initialFlipX = isRightCurrent;
        }

        /// <summary>
        /// 방향에 따른 스프라이트뒤집기 업데이트
        /// </summary>
        /// <param name="direction">방향</param>
        public void UpdateSpriteFlip(Vector2 direction)
        {
            if (direction == Vector2.zero)
            {
                return;
            }

            //스케일만 먹힘//나미지는 삭제
            if (direction.x > 0 && !isRightCurrent)//왼쪽
            {
                FlipScale();
            }
            else if (direction.x < 0 && isRightCurrent)//오른쪽
            {
                FlipScale();
            }
        }

        public void UpdateSpriteRotateFlip(Vector2 direction, float turnSpeed)
        {
            Quaternion targetRotate = Quaternion.identity;
            //체크하기
            if (direction.x > 0 && !isRightCurrent)//왼쪽
            {
                targetRotate = Quaternion.Euler(0, 0, 0);
            }
            else if (direction.x < 0 && isRightCurrent)//오른쪽
            {
                targetRotate = Quaternion.Euler(0, 180, 0);
            }

            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotate, turnSpeed * Time.deltaTime);
        }
    }
}