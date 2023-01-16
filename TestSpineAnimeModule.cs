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
        //�ظ���
        //������API���� �߰� ����� + �ʿ��ѱ�ɸ� ���� �ھƹ�����
        //ũ�� �������� �����ٵ� ����� �Ѱ��� �ھƹ�����
        //��Ʈ����Ʈ, �ΰ��ӿ�

        //-=�����ձ��
        //1. ��Ų����� (��)
        //2. �ִϸ��̼ǵ� �̸���������//��� ��üũ (��)
        //3. �ִϸ��̼� ����//Ʈ��, ���, ����, ���� ��� (��)
        //4. �ִϸ��̼� ��� ��������//��ҵ� �� ���� �޽� ��� �� ����ġ��Ʈ//IK�������� ������ ����(��)
        //5. ��������//�Ź� ���� ���°� ���ü� ������ �ڵ����̱׷��̼�ó���ؾߵ�(����)
        //6. �ִϸ��̼�//String���� Enumó�� �����̾��µ�//��ũ��Ʈ�ڵ������� �������ߵ�.//=>������ �����ξ��� �ڵ���ũ��Ʈ���� ��������(���߿�)
        //7. �ִ� ó���� ���� üũ(��)
        //8. �ִ��̸��� ���� ����, �и�ó��(��)

        //-=����
        //��������ϴ� �ִԿ� ���� ó������� �ʹ� �ٺ�ȭ��
        //=>ũ�Ժ��� ��Ʈ����Ʈ�κ��̶� �ΰ��Ӻκ�
        //Live2D�� AnyPotrait�� ��Ʈ�ѷ��� �־ �װ� ����ϸ� �Ǳ��ϴµ� �������� ���� ������ �ؾߵ�
        //=>Live2D�� ���� ������ �������� ó���ϴ°�
        //�ΰ����ϰ� ��Ʈ����Ʈ�� �и��ؾߵ�
        //=>�׷� ���� �̰� Base�� ��ɸ� �ִ°ɷ� �ϰ� ���ó���ؼ� ���� �۵��ǰ� ó���ϴ°� �¾ƺ���

        //-=�䱸����
        //�ִԹ���� �����ϴ� ���� ���ϱ�(���� �߿�)
        //UI�� ������ �ִ� ����
        //����ó�� ���� ��Ī ���� �ο�
        //(������ �Ⱥ��̴µ�)

        //-=���ּ̹���
        //��ǥ(�������� ���)_�����¿�յ�_���ľ�
        //XXXX_YYYY_ZZZZ//X=>����//Y, Z=>����

        //-=�ִ��� �ڵ�� �ű�� ó���ؾߵǴ� ����
        //1.�����Ͼִ� 2.Ʈ���켱����ó�� 3.���̹�

        //-=*�ΰ��� & ��Ʈ����Ʈ
        //1.�̸��(Eye, Mouse, Eyebrow,���) 2.�ٵ��� �� ������ ���� 3.��������

        /// <summary>
        /// �ִ����� ������
        /// </summary>
        public class AnimDefineInfoData
        {
            public string name;//�۵��� �̸�ó��//Enum���� ���� üũ//Enum�� �ڵ�ȭ�� �ʿ��ؼ� ��

            [Header("�����ξִ� ����")]
            public SpineAnimSetData[] spineAnimSetDataArray = new SpineAnimSetData[0];
        }

        

        /// <summary>
        /// �����ξִ� ������(�μ�ǰ)
        /// </summary>
        public class SpineAnimSetData
        {
            [SpineAnimation]
            public string spineAnim;//�����ξִ��̸�
            public int tracknumber;//Ʈ������
        }

        /// <summary>
        /// ������ ������ �����Ű�� ���� ������
        /// </summary>
        public class SpinePartChangeInfo
        {
            [SpineSlot]
            public string slot;//������ ����
            [SpineAttachment]
            public string attackment;//������ ����ġ
        }


        /// <summary>
        /// �ִϸ��̼� �޼��� Ÿ��//�̸����ǿ� ���� �ڵ���ũ��Ʈ ���� ���
        /// </summary>
        public enum AnimationMessageType
        {
            //�ٵ�
            None,
            Idle_Default,
            Idle_Wait,
            Walk,
            Run,
            Attack,
            Skill,
            Aim,//�켱���� ����
        }

        public enum EmotionAnimType
        {
            //�̸��
            //���� �ΰ����� ȥ�յǼ� ��������
            //������ ǥ���Ҽ� ������ �̰Ű����� ���� 
            //������ �ʿ�
            //ũ�� ��� �ϴ����� �з�
            //��� : ����, �� �� �̰��κ�
            //�ϴ� : ��, ȫ��(��?), �� �� �� ��ܺ��� �Ʒ��κ�


            //ȫ��
            //â��
            //����
            //��
            //���

            //-=�Ը�翡 ���� ��ȭ
            //���̿쿡��
            //A I  U  E O
            //ǥ�� �������� ��� ���� �����°ɷ� �����.
            //���ش� ��ȭ�� ��ȭ������ �Ը�纯ȭ�� ���. 

            //Up_Idle

            //Down_
            Idle,
            Smile,
        }


        //-=�ذ������- ��Ʈ����Ʈ(��)
        //1. Ʈ�������� 2���� �������� ó���� ����ߵ�.//�ִ� Ʈ���� ����� ���ߵ�//�����ο����� 5������ ����//(��)���Ѿ���� Ʈ��20000���� �����ؼ� üũ
        //2. ��Ʈ����Ʈ�� IK�����ʿ�//���콺�����Ϳ� ���� ���⼺ó���� �ֵ� ����//���� ������ġ���� ���콺�� �̵������� üũ�Ͽ�ó��(ó���� ������Ʈ�� �ʿ�)
        //3. �ִϸ��̼��� �����¿� ���󺸴� ������ ����� Ʈ���� ȥ�� => �߾���ġ������ ��Ʈ����Ʈĳ������ġ���� ���콺�������� IK ����üũ(ó���� ������Ʈ�� �ʿ�)


        //-=�ذ������- �ΰ���
        //1. ���⽽�Կ� ���� ó��(��)(3���� ����.�̰� �����ο��� ����־���� ������. ��Ʋ�� ������ ������ ����.)
        //2. �����ִ� ���� ����ó�� (��)
        //3. Ŀ���͸���¡ ����� ����// Ư���̹����� ���� ��Ʋ�� ��� ���� (��)(üũ�� => �޽�����ġ��Ʈ ���� ���ٰ� ��� => �׽�Ʈ��� �ȵ�. �ٸ��� ó��)
        //4. �ٸ�IK���� �����ͼ� Ray&circle�� ���� (��. �ʿ�� ����)
        //5. ��IK�� ���� ���⿡ ���� Ray&circle ����(��. �ʿ�� ����)
        //6. �� �������̵�ó�� üũ(��. SkeletonUtilityBone���� Override�����ϱ�)
        //7. ��(��)
        //8. Ư����ü => �� �ȷ��� üũ(��)
        //9. �� => Ư����ü �ȷ��� üũ(��)


        //-=�ع��� üũ
        //�� �����ʿ� ������ ����Ű�����
        //���ø����� �������� �ͱ��ѵ�
        //�̰� �ִ������ʰ� �����ؼ� �ذ��ؾ��� ����
        //������ �ִϸ��̼��� ��F�� ������Ŀ� ���� �������� �ٲ�� ������


        //-=��API����
        //��κ� String���� ó���Ǵ°��� �ֱ��ѵ� String���� ó���ϸ� �� ����//�׷��� ���κ��� ��ųʸ� ó��������//��
        //SkeletonAnimation�ȿ� ���� �������� ã�°� ��¦ ���ŷο�
        //�׳� ����

        public SkeletonAnimation anim;//����� �� �����ξִ� ������

        
        //�ִϸ��̼� ������ ����
        public AnimationReferenceAsset animRef;
        public AnimationReferenceAsset[] animationRefArray;

        //�̸����� �ִϸ��̼�
        public string animName;
        public string[] animNameArray;

        //��Ų
        //�̸����� �����;ߵɵ���//�ø������¡�ȵ��־ �ν����Ϳ��� �Ⱥ���
        //�ƴϸ� �ʱ⼼���Ҷ� ����ִ°͵� ����//��ųʸ���
        public string[] skinNameArray;
        public Skin[] skinArray;

        [Header("��Ʈ����Ʈ üũ")]

        [SpineSlot]
        public string tempAnim0;
        [SpineSlot]
        public string tempAnim1;
        [SpineBone]
        public string tempAnim2;
        [SpineAttachment]
        public string tempAnim3;
        [SpineAttachment(true, slotField: "WeaponSlot")]//�Ⱥ���
        public string tempAnim4;
        [SpineSkin]
        public string tempAnim5;
        [SpineAnimation]
        public string tempAnim6;
        [SpineAnimation("Idle")]//Ư���ִ� ��������
        public string tempAnim7;


        public Attachment targetCheck;


        public void GetSkeletonData()
        {
            //������ �ǲ� ���ܳ���
            SkeletonData data = anim.SkeletonDataAsset.GetAnimationStateData().SkeletonData;

        }
        
        public void ChangeSlotImage()
        {
            //�̿�
            //���÷��浥���� ���³��� ������ ��Ʋ��
            AtlasAssetBase atlasAssetBase = anim.SkeletonDataAsset.atlasAssets[0];
            Atlas atlas = atlasAssetBase.GetAtlas();


            //��Ʋ�󽺿� �ִ� �� ������ �������� �� ����
            //���⿡�� ��θ� �����ͼ� ��ü
            AtlasRegion atlasRegion = atlas.Regions[0];//�̰� 19���ΰ� ������//����//�ִϵ����Ϳ� ���� �� �ٸ��Ƿ� �Ź� �����
            //atlasRegion.name;//���

            //������ �ִ� �̹����� ��θ� ã�ƾߵ�
            //����/�̹���
            //����/��Ų/�̹���(�޽�)
            //��


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


            //������ ��Ÿ�ӿ����� �۵�
            anim.skeleton.SetSkin("Blue");

            //���� ��Ų���� ���ο� ���������� ���� ��Ų�� �����
            //anim.skeleton.Skin.AddSkin();

            //if (anim.skeleton.Skin.Name == "Green")
            //{

            //}
            //else
            //{
            //    anim.skeleton.SetSkin("Green");//������ ��Ÿ�ӿ����� �۵�
            //}

            //anim.skeleton.SetSkin("Green"); // 1. Ȱ�� ��Ų ����
            //anim.skeleton.SetSlotsToSetupPose(); // 2. ���� ��� ����Ͽ� �⺻ ����ġ��Ʈ�� �����Ͻʽÿ�.
            //anim.AnimationState.Apply(anim.skeleton); // 3. AnimationState�� ����Ͽ� ���� �����ӿ��� Ȱ�� ����ġ��Ʈ�� �����Ͻʽÿ�
        }

        public void MixSetting()
        {
            //�ͽ���������(����)
            SkeletonData data = anim.SkeletonDataAsset.GetAnimationStateData().SkeletonData;

            AnimationStateData stateData = new AnimationStateData(data);
            stateData.DefaultMix = 0.1f;//�⺻��ó��

            //�� �ִϸ��̼ǿ� ���� �������� ó��
            stateData.SetMix("walk", "jump", 0.2f);
            stateData.SetMix("jump", "walk", 0.4f);
            stateData.SetMix("jump", "run", 0.25f);
            stateData.SetMix("walk", "shoot", 0f);

            //Ȯ���ؾߵ�
            AnimationState state = new AnimationState(stateData);
        }

        public void GetAnimData()
        {
            //�ִ� �����Ͱ�������
            SkeletonData data = anim.SkeletonDataAsset.GetAnimationStateData().SkeletonData;

            //���� �ִϸ��̼� �̸� ��������
            animName = anim.AnimationName;

            //������ �ִϸ��̼ǵ鰡����
            //data.Animations.Count//��Ī���� Count ���Ӱ� ó���س�
            animNameArray = new string[data.Animations.Count];
            int temp = 0;
            foreach (var anim in data.Animations)
            {
                animNameArray[temp] = anim.Name;
                temp++;
            }

            //�ִϸ��̼����� ���°��� ��� ���� �����ؼ� ������ذŶ�
            //���÷��浥���� ���ο��� ���������ʴ� �������̴�.
            //���� �ν����Ϳ����Ϳ��� ������ ����־��ٰ�
            //���α����� ���� String���� AnimRef�� ������

            //animRef = data.Animations;
            //data.Animations.
            //animationRefArray
        }

        public void ResetAnimation()
        {
            anim.ClearState();//GetComponent�� ����
            anim.state.ClearTracks();//���� ȣ�����ִ°� ���ƺ���
        }

        public void PlayAnimation()
        {
            //�ִϸ��̼����� ������ ������//����üũ�� �ʿ��غ��̴µ� 
            //�ִϸ��̼����� ���������� ���°� ������ ����

            //Set�ϰ� Add�� ���̴� 
            //Set�� ���� ó���� ���ִ� ���̰�
            //Add�� �ִϸ��̼� ť�� �׾��ִ°�
            //Emtpy �� �ִ��� ����ִ°�


            //�۵��Ǵ� ����� Ʈ�� �ѹ��� ���� ����, ���� Ʈ�� �ѹ��� �ִϸ��̼� ���� ���� �������� �������.
            //������ 1 Ʈ�� ����� ���̷����� ��ü bone�� ����Ѵٸ�(��� key�� �����ִٸ�) 0 Ʈ�� ����� ������ �ʰ� 1 Ʈ�� ��Ǹ� ���ɴϴ�.

            //�׷��� ���� ��� �޸��鼭 ������ ���, ���� ����� ��ü ������ ����� ����� ��ü ������ key�� ���� �ʽ��ϴ�.
            //�׷��� 0���� �޸��Ⱑ ����Ǵ� ���߿� ����(1�� Ʈ��)�� �����ų ���, ��ü�� �޸��� ������ ��ü�� ���ݸ���� ������ ���� �� �� �ֽ��ϴ�.



            string temp = "";//�ִϸ��̼� ������

            //����ִϸ��̼��� ������Ű�� ���Ӱ� ������ �ִϸ��̼��� �缺//�ͽ�(����)�����Ͱ� ������ ȥ���� �ȵ�//�ͽ� == ����
            //Ʈ��, �ִϸ��̼� ������, ��������
            anim.AnimationState.SetAnimation(0, temp, false);

            //�ִϸ��̼� ť�� �߰����Ѽ� �ִϸ��̼��� �۵�
            //delay�� 0 �����̸� �ͽ������Ϳ� �ִ� �ͽ����ӽð�//�ʰ��̸� �״�� �����ð�
            //�����Ͻ� �ѹ������� �۵�
            //Ʈ��, �ִϸ��̼� ������, ��������, delay
            anim.AnimationState.AddAnimation(0, temp, false, 0.5f);

            //��ִϸ��̼��� ���
            //�ش� Ʈ���� �۵��Ǵ� �ִϸ��̼��� ��� ����� �۵������ʰ� ����.
            //anim.AnimationState.SetEmptyAnimation(,)

            //Ʈ��������//Ʈ�� ���� �ִϸ��̼� ����� ���� ���� �� ��Ÿ ���¸� ����            
            TrackEntry trackEntry = anim.AnimationState.SetAnimation(0, temp, false);




            //�߻�������//����

            //anim.AnimationState.SetAnimation(1, shoot, false);
            //anim.AnimationState.AddEmptyAnimation(1, 0.5f, 2f);

            //if (currentHealth > 0)
            //{
            //    //ü���� ����������
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




        //��Ʈ����Ʈ�ִϸ��̼��߿� �� ���ڰŸ��� �ִ��� �۵� ����
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


        //�ִϸ��̼� ó�����
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

        //���������//����
        public SkeletonAnimation skeletonAnimation;
        [SpineEvent(dataField: "skeletonAnimation", fallbackToTextField: true)]
        public string eventName;//�ش罺�÷���ִϸ��̼ǿ��� �̺�Ʈ�̸� üũ

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
            //���Ӱ���
            var mousePosition = Input.mousePosition;
            var worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            var skeletonSpacePoint = skeletonAnimation.transform.InverseTransformPoint(worldMousePosition);
            skeletonSpacePoint.x *= skeletonAnimation.Skeleton.ScaleX;
            skeletonSpacePoint.y *= skeletonAnimation.Skeleton.ScaleY;
            //bone.SetLocalPosition(skeletonSpacePoint);//IKBone
        }

        public void Turn(bool facingLeft)
        {
            //�ִϸ��̼� ������ȯ//�ܺο��� Ʈ���Ž����� ���� �������ֱ�
            skeletonAnimation.Skeleton.ScaleX = facingLeft ? -1f : 1f;
        }

        private static List<RaycastHit2D> checkIKList = new List<RaycastHit2D>();
        public void FootIKCheck()
        {
            //�߹ٴڿ� ���̴� IK ���
            //Ray���ٴ� Circle�� ������ ������//����ϱ�
            //Circle�� ���ƺ���

            //���� ������Ʈ ����//��������� ���Ĺ�����
            //SkeletonUtilityGroundConstraint
            //SkeletonUtilityBone
            


            //SkeletonUtilityBone������Ʈ �ʿ�
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
        /// Ÿ���� ���� �ȷο� ���
        /// </summary>
        /// <param name="followBone">�ȷο��� ��</param>
        /// <param name="targetTr">�i�ư� Ÿ��</param>
        /// <param name="isMove">������ �ȷο쿩��</param>
        /// <param name="isRotate">ȸ�� �ȷο쿩��</param>
        public static void TargetForBoneFollow(Bone followBone, Transform targetTr, bool isMove, bool isRotate)
        {
            //BoneFollowerGraphic ������Ʈ����
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
        /// ���� Ÿ���� �ȷο� ���
        /// </summary>
        /// <param name="followTarget">�ȷο��� Ÿ��</param>
        /// <param name="targetBone">�i�ư� ��</param>
        /// <param name="isMove">������ �ȷο쿩��</param>
        /// <param name="isRotate">ȸ�� �ȷο쿩��</param>
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
            //���̾������� ��������ε� üũ

            //1. �Ϲ����� ����Ƽ������Ʈ�� ó��
            MeshRenderer meshRenderer = anim.GetComponent<MeshRenderer>();

            //2. �����ο��� �� ������Ʈ�� ó��
            //SkeletonRenderSeparator(�������и�ü), SkeletonPartsRenderer(�и��� ������ �������ϱ�)
            //SkeletonPartsRenderer���� Orderby ������ ����
            //������ ��ο������ ������ ���氡��
            //http://ko.esotericsoftware.com/spine-unity#SkeletonRenderSeparator

            Slot slot = anim.skeleton.FindSlot("");
            SkeletonRenderSeparator skeletonRenderSeparator = SkeletonRenderSeparator.AddToSkeletonRenderer(anim);

            skeletonRenderSeparator.enabled = true;//��ο���� �и� Ȱ��
            skeletonRenderSeparator.enabled = false;//��ο���� �и� ��Ȱ��//���÷��淣�������� �켱������ �Ѿ

            //anim.separatorSlots.Remove(slot);
            anim.separatorSlots.Add(slot);

            //�̰��������鼭üũ�ؾ��ҵ���
            SkeletonPartsRenderer skeletonPartsRenderer = skeletonRenderSeparator.partsRenderers[0];
        }

        [ButtonMethod]
        public void CheckChange()
        {
            Slot slot = anim.skeleton.FindSlot("WeaponSlot");
            Attachment changeAttachment = slot.Skeleton.GetAttachment("WeaponSlot", "WeaponImage/Gun1");
            Attachment attachment = slot.Attachment;

            //���Ͻ���//�ٸ�����ġ���� ó��
            changeAttachment = slot.Skeleton.GetAttachment(tempAnim1, tempAnim3);//�ٲܴ��
            slot.Attachment = changeAttachment;//Nulló�����ص� �߹ٲ�

            //�ٸ�����//�ٸ�����ġ���� ó��//������ ������ �۵��� �߾ȵ�//������ ����� �ʿ� ����
            slot = anim.skeleton.FindSlot(tempAnim0);
            slot.Attachment = changeAttachment;
        }

        public void GetSpineInfoType()
        {
            SkeletonData data = anim.SkeletonDataAsset.GetAnimationStateData().SkeletonData;

            Bone bone = anim.skeleton.FindBone("");
            Slot slot = anim.skeleton.FindSlot("WeaponSlot");
            //slot.Attachment = null;//����ġ��Ʈ ����°�

            IkConstraint ikConstraint = anim.skeleton.FindIkConstraint("");
            //ikConstraint.Mix = 1;
            PathConstraint pathConstraint = anim.skeleton.FindPathConstraint("");
            TransformConstraint transformConstraint = anim.skeleton.FindTransformConstraint("");
            SpringConstraint springConstraint = anim.skeleton.FindSpringConstraint("");//��������


            //Ȯ���غ���//��Ų���� ���� �� üũ�ϱ�
            //����ġ��Ʈ�� ���ٴµ�//��� ��� ����//����ġ��Ʈ�� ���Ӱ� ���;ߵǴµ� ��� ���� �����
            //��� �����°ɱ�//����°� ������// �������� ���� �𸣰ڳ�
            //�ٸ��� ó�� �����ο��� ó�����ְ� ����ߵ�
            AtlasAssetBase atlasAssetBase = anim.SkeletonDataAsset.atlasAssets[0];
            Atlas atlas = atlasAssetBase.GetAtlas();
            AtlasRegion region = atlas.FindRegion("WeaponImage/Gun1");

            //AtlasAttachmentLoader atlasAttachmentLoader = new AtlasAttachmentLoader(atlas);

            //atlasAttachmentLoader.NewMeshAttachment(,,,);

            //anim.skeleton.SetAttachment("WeaponSlot", "WeaponImage/Gun2");
            //anim.skeleton.SetAttachment("Char","CharImage/Yellow" );

            //���Ծȿ� �ִ� ����
            Attachment attachment1 = anim.skeleton.GetAttachment("Char", "");

            //Attachment attachment2 = anim.skeleton.GetAttachment(0, "");//SkeletonDataAsset�� Slots�� �Ʒ����� �������� �۵���
            Attachment attachment4 = slot.Attachment;
        }

        [ButtonMethod]
        public void CheckFunc()
        {
            //������ ��� üũ��
            //SkeletonAnimation => �⺻������ ó��
            //SkeletonDataAsset => ��ũ���ͺ������Ʈ ������
            //AnimationReferenceAsset => �ִϸ��̼��� ��ũ���ͺ������Ʈ�� ���ִ� ����//���λ������༭ Data������ ����������������

        }
    }
}


