using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;
using AnimationState = Spine.AnimationState;
using EventData = Spine.EventData;
using Event = Spine.Event;

namespace lLCroweTool.AnimeSystem.Spine
{
    public class TestSpineAnimeModule : MonoBehaviour
    {
        //※목적
        //스파인API와의 중간 어댑터 + 필요한기능만 빼서 박아버리기
        //크게 두종류로 나눌텐데 현재는 한곳에 박아버리기
        //포트레이트, 인게임용

        //-=※통합기능
        //1. 스킨변경법 (완)
        //2. 애니메이션들 이름가져오기//방식 재체크 (완)
        //3. 애니메이션 조작//트랙, 재생, 정지, 블랜드 등등 (완)
        //4. 애니메이션 요소 가져오기//요소들 본 슬롯 메쉬 경로 등 어태치먼트//IK종류안의 값들을 변경(완)
        //5. 구역설계//매번 같은 형태가 나올수 없으니 자동마이그레이션처리해야됨(변경)
        //6. 애니메이션//String말고 Enum처리 예정이었는데//스크립트자동제작을 만들어줘야됨.//=>저번에 만들어두었던 자동스크립트제작 가져오기(나중에)
        //7. 애님 처리를 위한 체크(완)
        //8. 애님이름에 대한 정의, 분리처리(완)

        //-=문제
        //만들고자하는 애님에 따라 처리방식이 너무 다분화됨
        //=>크게보면 포트레이트부분이랑 인게임부분
        //Live2D와 AnyPotrait는 컨트롤러가 있어서 그걸 사용하면 되긴하는데 스파인은 따로 제작을 해야됨
        //=>Live2D로 하지 않으니 부위마다 처리하는게
        //인게임하고 포트레이트는 분리해야됨
        //=>그럼 지금 이건 Base에 기능만 있는걸로 하고 상속처리해서 따로 작동되게 처리하는게 맞아보임

        //-=요구사항
        //애님방식을 정의하는 룰을 정하기(가장 중요)
        //UI나 간단한 애님 빼고
        //제스처에 따라 명칭 룰을 부여
        //(명세서가 안보이는데)

        //-=네이밍순서
        //목표(실질적인 대상)_상하좌우앞뒤_수식어
        //XXXX_YYYY_ZZZZ//X=>메인//Y, Z=>서브

        //-=애님을 코드로 옮길시 처리해야되는 구역
        //1.움직일애님 2.트랙우선순위처리 3.네이밍

        //-=*인게임 & 포트레이트
        //1.이모션(Eye, Mouse, Eyebrow,등등) 2.바디의 각 움직일 파츠 3.제약조건

        /// <summary>
        /// 애님정의 데이터
        /// </summary>
        public class AnimDefineInfoData
        {
            public string name;//작동될 이름처리//Enum으로 할지 체크//Enum은 자동화가 필요해서 흠

            [Header("스파인애님 설정")]
            public SpineAnimSetData[] spineAnimSetDataArray = new SpineAnimSetData[0];
        }

        

        /// <summary>
        /// 스파인애님 데이터(부속품)
        /// </summary>
        public class SpineAnimSetData
        {
            [SpineAnimation]
            public string spineAnim;//스파인애님이름
            public int tracknumber;//트랙관련
        }

        /// <summary>
        /// 스파인 파츠를 변경시키기 위한 데이터
        /// </summary>
        public class SpinePartChangeInfo
        {
            [SpineSlot]
            public string slot;//변경할 슬롯
            [SpineAttachment]
            public string attackment;//변경할 어태치
        }


        /// <summary>
        /// 애니메이션 메세지 타입//이름정의에 따른 자동스크립트 제작 대기
        /// </summary>
        public enum AnimationMessageType
        {
            //바디
            None,
            Idle_Default,
            Idle_Wait,
            Walk,
            Run,
            Attack,
            Skill,
            Aim,//우선순위 높음
        }

        public enum EmotionAnimType
        {
            //이모션
            //기억상 두가지가 혼합되서 여러가지
            //감정을 표현할수 있으니 이거관련은 따로 
            //연구가 필요
            //크게 상단 하단으로 분류
            //상단 : 눈썹, 눈 등 미간부분
            //하단 : 입, 홍조(볼?), 혀 등 코 상단부터 아래부분


            //홍조
            //창백
            //빨감
            //땀
            //등등

            //-=입모양에 따른 변화
            //아이우에오
            //A I  U  E O
            //표시 자음발음 경우 혀로 나오는걸로 기억함.
            //※해당 변화는 대화햇을시 입모양변화에 사용. 

            //Up_Idle

            //Down_
            Idle,
            Smile,
        }


        //-=※개별기능- 포트레이트(완)
        //1. 트랙조작을 2개씩 묶음으로 처리를 해줘야됨.//최대 트랙이 몇개인지 봐야됨//스파인에서는 5개까지 가능//(완)제한없어보임 트랙20000까지 세팅해서 체크
        //2. 포트레이트용 IK조작필요//마우스포인터에 따른 방향성처리가 주된 목적//본의 현재위치에서 마우스의 이동값들을 체크하여처리(처리할 프로젝트가 필요)
        //3. 애니메이션을 상하좌우 봐라보는 방향을 만들고 트랙을 혼합 => 중앙위치에서와 포트레이트캐릭터위치에서 마우스방향으로 IK 방향체크(처리할 프로젝트가 필요)


        //-=※개별기능- 인게임
        //1. 무기슬롯에 대한 처리(완)(3번과 연동.이거 스파인에서 집어넣어줘야 가능함. 아틀라스 변경이 문제가 있음.)
        //2. 보여주는 순서 관련처리 (완)
        //3. 커스터마이징 기능을 제작// 특정이미지에 대한 아틀라스 경로 변경 (완)(체크중 => 메쉬어태치먼트 생성 보다가 대기 => 테스트대기 안됨. 다르게 처리)
        //4. 다리IK본을 가져와서 Ray&circle와 연동 (완. 필요시 변경)
        //5. 손IK에 대한 방향에 대한 Ray&circle 연동(완. 필요시 변경)
        //6. 본 오버라이딩처리 체크(완. SkeletonUtilityBone에서 Override참조하기)
        //7. 턴(완)
        //8. 특정물체 => 본 팔로잉 체크(완)
        //9. 본 => 특정물체 팔로잉 체크(완)


        //-=※문제 체크
        //흠 설계쪽에 문제가 생길거같은데
        //템플릿으로 빼버리고 싶긴한데
        //이건 애님파츠쪽과 상의해서 해결해야할 느낌
        //스파인 애니메이션을 어덯게 만드느냐에 따라 동적으로 바뀔거 같은데


        //-=※API정보
        //대부분 String으로 처리되는곳이 있긴한데 String으로 처리하면 좀 느림//그런데 몇몇부분은 딕셔너리 처리되있음//흠
        //SkeletonAnimation안에 모든게 다있지만 찾는게 살짝 번거로움
        //그냥 쓰자

        public SkeletonAnimation anim;//대상이 될 스파인애님 데이터

        
        //애니메이션 참조로 에셋
        public AnimationReferenceAsset animRef;
        public AnimationReferenceAsset[] animationRefArray;

        //이름으로 애니메이션
        public string animName;
        public string[] animNameArray;

        //스킨
        //이름으로 가져와야될듯함//시리얼라이징안되있어서 인스팩터에서 안보임
        //아니면 초기세팅할때 집어넣는것도 좋음//딕셔너리로
        public string[] skinNameArray;
        public Skin[] skinArray;

        [Header("어트리뷰트 체크")]

        [SpineSlot]
        public string tempAnim0;
        [SpineSlot]
        public string tempAnim1;
        [SpineBone]
        public string tempAnim2;
        [SpineAttachment]
        public string tempAnim3;
        [SpineAttachment(true, slotField: "WeaponSlot")]//안보임
        public string tempAnim4;
        [SpineSkin]
        public string tempAnim5;
        [SpineAnimation]
        public string tempAnim6;
        [SpineAnimation("Idle")]//특정애님 강제지정
        public string tempAnim7;


        public Attachment targetCheck;


        public void GetSkeletonData()
        {
            //데이터 꽁꽁 숨겨났다
            SkeletonData data = anim.SkeletonDataAsset.GetAnimationStateData().SkeletonData;

        }
        
        public void ChangeSlotImage()
        {
            //미완
            //스컬레톤데이터 에셋내의 지정된 아틀라스
            AtlasAssetBase atlasAssetBase = anim.SkeletonDataAsset.atlasAssets[0];
            Atlas atlas = atlasAssetBase.GetAtlas();


            //아틀라스에 있는 각 파츠의 영역들이 들어가 있음
            //여기에서 경로를 가져와서 교체
            AtlasRegion atlasRegion = atlas.Regions[0];//이게 19개인거 같은데//맞음//애니데이터에 따라 다 다르므로 매번 변경됨
            //atlasRegion.name;//경로

            //기존에 있던 이미지의 경로를 찾아야됨
            //슬롯/이미지
            //슬롯/스킨/이미지(메쉬)
            //본


        }

        public void SetSkin()
        {
            SkeletonData data = anim.SkeletonDataAsset.GetAnimationStateData().SkeletonData;

            skinArray = new Skin[data.Skins.Count];
            skinNameArray = new string[data.Skins.Count];
            int count = 0;
            foreach (var item in data.Skins)
            {
                skinNameArray[count] = item.Name;
                skinArray[count] = item;
                count++;
            }


            //에디터 런타임에서만 작동
            anim.skeleton.SetSkin("Blue");

            //기존 스킨위에 새로운 제약조건을 가진 스킨을 덮어씌음
            //anim.skeleton.Skin.AddSkin();

            //if (anim.skeleton.Skin.Name == "Green")
            //{

            //}
            //else
            //{
            //    anim.skeleton.SetSkin("Green");//에디터 런타임에서만 작동
            //}

            //anim.skeleton.SetSkin("Green"); // 1. 활성 스킨 설정
            //anim.skeleton.SetSlotsToSetupPose(); // 2. 설정 포즈를 사용하여 기본 어태치먼트를 설정하십시오.
            //anim.AnimationState.Apply(anim.skeleton); // 3. AnimationState를 사용하여 현재 움직임에서 활성 어태치먼트를 설정하십시오
        }

        public void MixSetting()
        {
            //믹스설정관련(블랜딩)
            SkeletonData data = anim.SkeletonDataAsset.GetAnimationStateData().SkeletonData;

            AnimationStateData stateData = new AnimationStateData(data);
            stateData.DefaultMix = 0.1f;//기본값처리

            //각 애니메이션에 따라 동적으로 처리
            stateData.SetMix("walk", "jump", 0.2f);
            stateData.SetMix("jump", "walk", 0.4f);
            stateData.SetMix("jump", "run", 0.25f);
            stateData.SetMix("walk", "shoot", 0f);

            //확인해야됨
            AnimationState state = new AnimationState(stateData);
        }

        public void GetAnimData()
        {
            //애님 데이터가져오기
            SkeletonData data = anim.SkeletonDataAsset.GetAnimationStateData().SkeletonData;

            //현재 애니메이션 이름 가져오기
            animName = anim.AnimationName;

            //지정된 애니메이션들가져옴
            //data.Animations.Count//이칭구들 Count 새롭게 처리해놈
            animNameArray = new string[data.Animations.Count];
            int temp = 0;
            foreach (var anim in data.Animations)
            {
                animNameArray[temp] = anim.Name;
                temp++;
            }

            //애니메이션참조 에셋같은 경우 따로 생성해서 만들어준거라
            //스컬레톤데이터 내부에는 존재하지않는 데이터이다.
            //따로 인스팩터에디터에서 선언후 집어넣어줄것
            //내부구조를 보니 String보다 AnimRef가 더빠름

            //animRef = data.Animations;
            //data.Animations.
            //animationRefArray
        }

        public void ResetAnimation()
        {
            anim.ClearState();//GetComponent가 존재
            anim.state.ClearTracks();//따로 호출해주는게 좋아보임
        }

        public void PlayAnimation()
        {
            //애니메이션으로 대입이 가능함//성능체크가 필요해보이는데 
            //애니메이션으로 직접적으로 들어가는게 성능이 좋음

            //Set하고 Add의 차이는 
            //Set은 곧장 처리를 해주는 것이고
            //Add는 애니메이션 큐를 쌓아주는것
            //Emtpy 빈 애님을 집어넣는것


            //작동되는 방식은 트랙 넘버가 높을 수록, 낮은 트랙 넘버의 애니메이션 위에 덮어 씌워지는 방식이죠.
            //때문에 1 트랙 모션이 스켈레톤의 전체 bone을 사용한다면(모두 key가 잡혀있다면) 0 트랙 모션은 보이지 않고 1 트랙 모션만 나옵니다.

            //그래서 예를 들어 달리면서 공격일 경우, 공격 모션은 상체 부위만 모션을 만들고 하체 부위는 key를 주지 않습니다.
            //그러면 0번의 달리기가 실행되는 와중에 공격(1번 트랙)을 실행시킬 경우, 하체는 달리고 있지만 상체는 공격모션이 나오는 것을 볼 수 있습니다.



            string temp = "";//애니메이션 데이터

            //현재애니메이션을 중지시키고 새롭게 세팅한 애니메이션을 재성//믹스(블랜드)데이터가 없으면 혼합이 안됨//믹스 == 블랜드
            //트랙, 애니메이션 데이터, 루프여부
            anim.AnimationState.SetAnimation(0, temp, false);

            //애니메이션 큐에 추가시켜서 애니메이션을 작동
            //delay가 0 이하이면 믹스데이터에 있는 믹스지속시간//초과이면 그대로 지연시간
            //루프일시 한바퀴돌고 작동
            //트랙, 애니메이션 데이터, 루프여부, delay
            anim.AnimationState.AddAnimation(0, temp, false, 0.5f);

            //빈애니메이션일 경우
            //해당 트랙이 작동되는 애니메이션을 비게 만들어 작동되지않게 만듬.
            //anim.AnimationState.SetEmptyAnimation(,)

            //트랙진입점//트랙 에서 애니메이션 재생을 위한 설정 및 기타 상태를 저장            
            TrackEntry trackEntry = anim.AnimationState.SetAnimation(0, temp, false);




            //발사했을시//예시

            //anim.AnimationState.SetAnimation(1, shoot, false);
            //anim.AnimationState.AddEmptyAnimation(1, 0.5f, 2f);

            //if (currentHealth > 0)
            //{
            //    //체력이 남아있으면
            //    anim.AnimationState.SetAnimation(0, hit, false);
            //    anim.AnimationState.AddAnimation(0, idle, true, 0);
            //}
            //else
            //{
            //    if (currentHealth >= 0)
            //    {
            //        gauge.fillPercent = 0;
            //        anim.AnimationState.SetAnimation(0, death, false).TrackEnd = float.PositiveInfinity;
            //    }
            //}
        }




        //포트레이트애니메이션중에 눈 깜박거리는 애님을 작동 예시
        IEnumerator PlayBlinkAnim()
        {
            AnimationReferenceAsset blinkAnimation = null;
            float minimumDelay = 0.15f;
            float maximumDelay = 3f;
            int BlinkTrack = 0;

            var skeletonAnimation = GetComponent<SkeletonAnimation>(); if (skeletonAnimation == null) yield break;
            while (true)
            {
                skeletonAnimation.AnimationState.SetAnimation(BlinkTrack, blinkAnimation, false);
                yield return new WaitForSeconds(Random.Range(minimumDelay, maximumDelay));
            }
        }


        //애니메이션 처리방식
        IEnumerator DoDemoRoutine()
        {
            while (true)
            {
                //anim.AnimationState.SetAnimation(0, walkAnimationName, true);
                //yield return new WaitForSeconds(runWalkDuration);

                //anim.AnimationState.SetAnimation(0, runAnimationName, true);
                //yield return new WaitForSeconds(runWalkDuration);

                //// AddAnimation queues up an animation to play after the previous one ends.
                //anim.AnimationState.SetAnimation(0, runToIdleAnimationName, false);
                //anim.AnimationState.AddAnimation(0, idleAnimationName, true, 0);
                //yield return new WaitForSeconds(1f);

                //anim.skeleton.ScaleX = -1;
                //anim.AnimationState.SetAnimation(0, idleTurnAnimationName, false);
                //anim.AnimationState.AddAnimation(0, idleAnimationName, true, 0);
                //yield return new WaitForSeconds(0.5f);
                //anim.skeleton.ScaleX = 1;
                //anim.AnimationState.SetAnimation(0, idleTurnAnimationName, false);
                //anim.AnimationState.AddAnimation(0, idleAnimationName, true, 0);
                //yield return new WaitForSeconds(0.5f);

            }
        }

        //오디오관련//예시
        public SkeletonAnimation skeletonAnimation;
        [SpineEvent(dataField: "skeletonAnimation", fallbackToTextField: true)]
        public string eventName;//해당스컬레톤애니메이션에서 이벤트이름 체크

        [Space]
        public AudioSource audioSource;
        public AudioClip audioClip;
        public float basePitch = 1f;
        public float randomPitchOffset = 0.1f;

        EventData eventData;

        public void SoundSetting()
        {
            if (audioSource == null) return;
            if (skeletonAnimation == null) return;
            skeletonAnimation.Initialize(false);
            if (!skeletonAnimation.valid) return;

            eventData = skeletonAnimation.Skeleton.Data.FindEvent(eventName);


            skeletonAnimation.AnimationState.Event += 
            HandleAnimationStateEvent;
        }

        private void HandleAnimationStateEvent(TrackEntry trackEntry, Event e)
        {   
            if (eventData == e.Data)
            {
                audioSource.pitch = basePitch + Random.Range(-randomPitchOffset, randomPitchOffset);
                audioSource.clip = audioClip;
                audioSource.Play();
            }
        }


        public void Aim()
        {
            //에임관련
            var mousePosition = Input.mousePosition;
            var worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            var skeletonSpacePoint = skeletonAnimation.transform.InverseTransformPoint(worldMousePosition);
            skeletonSpacePoint.x *= skeletonAnimation.Skeleton.ScaleX;
            skeletonSpacePoint.y *= skeletonAnimation.Skeleton.ScaleY;
            //bone.SetLocalPosition(skeletonSpacePoint);//IKBone
        }

        public void Turn(bool facingLeft)
        {
            //애니메이션 방향전환//외부에서 트리거식으로 만들어서 적용해주기
            skeletonAnimation.Skeleton.ScaleX = facingLeft ? -1f : 1f;
        }

        private static List<RaycastHit2D> checkIKList = new List<RaycastHit2D>();
        public void FootIKCheck()
        {
            //발바닥에 쓰이는 IK 기능
            //Ray보다는 Circle이 괜찮지 않을까//고민하기
            //Circle이 나아보임

            //밑의 컴포넌트 참조//문제생길시 고쳐버리기
            //SkeletonUtilityGroundConstraint
            //SkeletonUtilityBone
            


            //SkeletonUtilityBone컴포넌트 필요
            ContactFilter2D contactFilter2D = new ContactFilter2D();
            float distance = 0;
            float yPos= 0;
            Vector2 offSet;
            float radius = 0.2f;

            if (Physics2D.CircleCast(transform.position, radius, Vector2.up, contactFilter2D, checkIKList, distance) < 1)
            {
                return;
            }

            if (Physics2D.Raycast(transform.position,Vector2.down, contactFilter2D, checkIKList, distance) < 1)
            {
                return;
            }

            for (int i = 0; i < checkIKList.Count; i++)
            {
                RaycastHit2D temp = checkIKList[i];
                if (temp.collider)
                {
                    yPos = temp.point.y;
                    break;
                }
            }

            //yPos = Mathf.MoveTowards(lastHitY, transform.position.y, adjustDistanceThisFrame);

            //bone.bone.X = transform.localPosition.x / hierarchy.PositionScale;
            //bone.bone.Y = transform.localPosition.y / hierarchy.PositionScale;
        }

        /// <summary>
        /// 타겟이 본을 팔로우 기능
        /// </summary>
        /// <param name="followBone">팔로우할 본</param>
        /// <param name="targetTr">쫒아갈 타겟</param>
        /// <param name="isMove">움직임 팔로우여부</param>
        /// <param name="isRotate">회전 팔로우여부</param>
        public static void TargetForBoneFollow(Bone followBone, Transform targetTr, bool isMove, bool isRotate)
        {
            //BoneFollowerGraphic 컴포넌트참조
            if (isMove)
            {
                targetTr.position = new Vector2(followBone.WorldX, followBone.WorldY);
            }
            if (isRotate)
            {
                targetTr.rotation = followBone.GetQuaternion();
            }
        }

        /// <summary>
        /// 본이 타겟을 팔로우 기능
        /// </summary>
        /// <param name="followTarget">팔로우할 타겟</param>
        /// <param name="targetBone">쫒아갈 본</param>
        /// <param name="isMove">움직임 팔로우여부</param>
        /// <param name="isRotate">회전 팔로우여부</param>
        public static void BoneForTargetFollow(Transform followTarget, Bone targetBone, bool isMove, bool isRotate)
        {
            if (isMove)
            {
                Vector2 pos = followTarget.position;
                targetBone.WorldX = pos.x;
                targetBone.WorldY = pos.y;
            }
            if (isRotate)
            {
                targetBone.RotateWorld(followTarget.rotation.z);
            }
        }

        public void SetOrderBy(int value)
        {
            //레이어정렬이 여러방식인데 체크

            //1. 일반적인 유니티컴포넌트로 처리
            MeshRenderer meshRenderer = anim.GetComponent<MeshRenderer>();

            //2. 스파인에서 준 컴포넌트로 처리
            //SkeletonRenderSeparator(랜더러분리체), SkeletonPartsRenderer(분리된 랜더링 순서정하기)
            //SkeletonPartsRenderer에서 Orderby 순서를 지정
            //정해진 드로우오더를 강제로 변경가능
            //http://ko.esotericsoftware.com/spine-unity#SkeletonRenderSeparator

            Slot slot = anim.skeleton.FindSlot("");
            SkeletonRenderSeparator skeletonRenderSeparator = SkeletonRenderSeparator.AddToSkeletonRenderer(anim);

            skeletonRenderSeparator.enabled = true;//드로우오더 분리 활성
            skeletonRenderSeparator.enabled = false;//드로우오더 분리 비활성//스컬레톤랜더러한테 우선순위가 넘어감

            //anim.separatorSlots.Remove(slot);
            anim.separatorSlots.Add(slot);

            //이건직접보면서체크해야할듯함
            SkeletonPartsRenderer skeletonPartsRenderer = skeletonRenderSeparator.partsRenderers[0];
        }

        [ButtonMethod]
        public void CheckChange()
        {
            Slot slot = anim.skeleton.FindSlot("WeaponSlot");
            Attachment changeAttachment = slot.Skeleton.GetAttachment("WeaponSlot", "WeaponImage/Gun1");
            Attachment attachment = slot.Attachment;

            //동일슬롯//다른어태치에서 처리
            changeAttachment = slot.Skeleton.GetAttachment(tempAnim1, tempAnim3);//바꿀대상
            slot.Attachment = changeAttachment;//Null처리안해도 잘바뀜

            //다른슬롯//다른어태치에서 처리//지정은 되지만 작동은 잘안됨//어차피 여기는 필요 없음
            slot = anim.skeleton.FindSlot(tempAnim0);
            slot.Attachment = changeAttachment;
        }

        public void GetSpineInfoType()
        {
            SkeletonData data = anim.SkeletonDataAsset.GetAnimationStateData().SkeletonData;

            Bone bone = anim.skeleton.FindBone("");
            Slot slot = anim.skeleton.FindSlot("WeaponSlot");
            //slot.Attachment = null;//어태치먼트 지우는거

            IkConstraint ikConstraint = anim.skeleton.FindIkConstraint("");
            //ikConstraint.Mix = 1;
            PathConstraint pathConstraint = anim.skeleton.FindPathConstraint("");
            TransformConstraint transformConstraint = anim.skeleton.FindTransformConstraint("");
            SpringConstraint springConstraint = anim.skeleton.FindSpringConstraint("");//아직없음


            //확인해보니//스킨별로 따로 들어감 체크하기
            //어태치먼트가 없다는데//어디 빈거 같다//어태치먼트를 새롭게 들고와야되는데 없어서 문제 생긴것
            //어디서 들고오는걸까//만드는거 같은데// 시퀸스가 뭔지 모르겠네
            //다르게 처리 스파인에서 처리해주고 해줘야됨
            AtlasAssetBase atlasAssetBase = anim.SkeletonDataAsset.atlasAssets[0];
            Atlas atlas = atlasAssetBase.GetAtlas();
            AtlasRegion region = atlas.FindRegion("WeaponImage/Gun1");

            //AtlasAttachmentLoader atlasAttachmentLoader = new AtlasAttachmentLoader(atlas);

            //atlasAttachmentLoader.NewMeshAttachment(,,,);

            //anim.skeleton.SetAttachment("WeaponSlot", "WeaponImage/Gun2");
            //anim.skeleton.SetAttachment("Char","CharImage/Yellow" );

            //슬롯안에 있는 존재
            Attachment attachment1 = anim.skeleton.GetAttachment("Char", "");

            //Attachment attachment2 = anim.skeleton.GetAttachment(0, "");//SkeletonDataAsset의 Slots에 아래에서 윗순으로 작동함
            Attachment attachment4 = slot.Attachment;
        }

        [ButtonMethod]
        public void CheckFunc()
        {
            //구조및 기능 체크중
            //SkeletonAnimation => 기본적으로 처리
            //SkeletonDataAsset => 스크립터블오브젝트 데이터
            //AnimationReferenceAsset => 애니메이션을 스크립터블오브젝트로 되있는 상태//새로생성해줘서 Data내에는 따로존재하지않음

        }
    }
}


