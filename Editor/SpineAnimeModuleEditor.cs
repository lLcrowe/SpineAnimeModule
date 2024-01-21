#if UNITY_EDITOR
using lLCroweTool.AnimeSystem.Spine;
using Spine;
using Spine.Unity;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Spine.Unity.Editor.SpineEditorUtilities;
using Animation = Spine.Animation;
using AnimationState = Spine.AnimationState;

namespace lLCroweTool.QC.EditorOnly
{
    [CustomEditor(typeof(SpineAnimeModule_FuncBase), true)]
    //[CustomEditor(typeof(SpineAnimeModule_FuncBase), true)]
    [CanEditMultipleObjects]

    public class SpineAnimeModuleEditor : Editor
    {
        private static bool showAnimationList = true;
        private GUIStyle activePlayButtonStyle, idlePlayButtonStyle;


        private Transform targetTr;
        List<Spine.Event> currentAnimationEvents = new List<Spine.Event>();
        List<float> currentAnimationEventTimes = new List<float>();
        internal bool requiresRefresh;
        float editorDeltaTime;
        double lastTimeSinceStartup;
        private void OnEnable()
        {
            var value = target as SpineAnimeModule_FuncBase;
            targetTr = value.transform;
            EditorApplication.update += UpdateSceneView;
        }

        private void OnDisable()
        {
            EditorApplication.update -= UpdateSceneView;
        }


        private void UpdateSceneView()
        {
            //강제업데이트
            EditorUtility.SetDirty(targetTr);

            //deltaTime
            var startUpTime = EditorApplication.timeSinceStartup;
            editorDeltaTime = (float)(startUpTime - lastTimeSinceStartup);
            lastTimeSinceStartup = startUpTime;

            foreach (var item in targets)
            {
                var func = item as SpineAnimeModule_FuncBase;
                if (func == null)
                {
                    return;
                }

                var animation = func.skeletonAnimation;
                if (animation.SkeletonDataAsset == null)
                {
                    return;
                }

                animation.Update(editorDeltaTime);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            foreach (var item in targets)
            {
                var func = item as SpineAnimeModule_FuncBase;
                if (func == null)
                {
                    return;
                }

                var animation = func.skeletonAnimation;
                if (animation.SkeletonDataAsset == null)
                {
                    return;
                }

                UpdateInspector(animation);
            }
        }


        private void UpdateInspector(SkeletonAnimation skeletonAnimation)
        {
            if (skeletonAnimation == null)
            {
                return;
            }

            { // Lazy initialization because accessing EditorStyles values in OnEnable during a recompile causes UnityEditor to throw null exceptions. (Unity 5.3.5)
                idlePlayButtonStyle = idlePlayButtonStyle ?? new GUIStyle(EditorStyles.miniButton);
                if (activePlayButtonStyle == null)
                {
                    activePlayButtonStyle = new GUIStyle(idlePlayButtonStyle);
                    activePlayButtonStyle.normal.textColor = Color.red;
                }
            }

            DrawAnimationList(skeletonAnimation.SkeletonDataAsset.GetSkeletonData(false), skeletonAnimation);
        }

        private bool IsValid(SkeletonAnimation skeletonAnimation)
        {
            return skeletonAnimation != null && skeletonAnimation.valid;
        }

        public TrackEntry ActiveTrack(SkeletonAnimation skeletonAnimation)
        {
            return IsValid(skeletonAnimation) ? skeletonAnimation.AnimationState.GetCurrent(0) : null; 
        }
        void DrawAnimationList(SkeletonData targetSkeletonData, SkeletonAnimation skeletonAnimation)
        {
            showAnimationList = EditorGUILayout.Foldout(showAnimationList, TempContent(string.Format("Animations [{0}]", targetSkeletonData.Animations.Count), Icons.animationRoot));
            if (!showAnimationList)
                return;

            bool isPreviewWindowOpen = IsValid(skeletonAnimation);

            if (isPreviewWindowOpen)
            {
                if (GUILayout.Button(TempContent("Setup Pose", Icons.skeleton), GUILayout.Width(105), GUILayout.Height(18)))
                {
                    ClearAnimationSetupPose(skeletonAnimation);
                    RefreshOnNextUpdate();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Animations can be previewed if you expand the Preview window below.", MessageType.Info);
            }

            EditorGUILayout.LabelField("Name", "      Duration");
            //bool nonessential = targetSkeletonData.ImagesPath != null; // Currently the only way to determine if skeleton data has nonessential data. (Spine 3.6)
            //float fps = targetSkeletonData.Fps;
            //if (nonessential && fps == 0) fps = 30;

            TrackEntry activeTrack = ActiveTrack(skeletonAnimation);
            foreach (Animation animation in targetSkeletonData.Animations)
            {
                using (new GUILayout.HorizontalScope())
                {
                    if (isPreviewWindowOpen)
                    {
                        bool active = activeTrack != null && activeTrack.Animation == animation;
                        //bool sameAndPlaying = active && activeTrack.TimeScale > 0f;
                        if (GUILayout.Button("\u25BA", active ? activePlayButtonStyle : idlePlayButtonStyle, GUILayout.Width(24)))
                        {
                            PlayPauseAnimation(targetSkeletonData, skeletonAnimation, animation.Name, true);
                            activeTrack = ActiveTrack(skeletonAnimation);
                        }
                    }
                    else
                    {
                        GUILayout.Label("-", GUILayout.Width(24));
                    }
                    
                    string durationString = animation.Duration.ToString("f3");
                    EditorGUILayout.LabelField(new GUIContent(animation.Name, Icons.animation), TempContent(durationString + "s", tooltip: string.Format("{0} seconds\n{1} timelines", durationString, animation.Timelines.Count)));
                }
            }
        }

        public void PlayPauseAnimation(SkeletonData skeletonData, SkeletonAnimation skeletonAnimation, string animationName, bool loop)
        {
            if (skeletonData == null) return;

            if (skeletonAnimation == null)
            {
                Debug.LogWarning("Animation was stopped but preview doesn't exist. It's possible that the Preview Panel is closed.");
                return;
            }

            if (!skeletonAnimation.valid) return;

            if (string.IsNullOrEmpty(animationName))
            {
                skeletonAnimation.Skeleton.SetToSetupPose();
                skeletonAnimation.AnimationState.ClearTracks();
                return;
            }

            Animation targetAnimation = skeletonData.FindAnimation(animationName);
            if (targetAnimation != null)
            {
                TrackEntry currentTrack = this.ActiveTrack(skeletonAnimation);
                bool isEmpty = (currentTrack == null);
                bool isNewAnimation = isEmpty || currentTrack.Animation != targetAnimation;

                Skeleton skeleton = skeletonAnimation.Skeleton;
                AnimationState animationState = skeletonAnimation.AnimationState;

                if (isEmpty)
                {
                    skeleton.SetToSetupPose();
                    animationState.SetAnimation(0, targetAnimation, loop);
                }
                else
                {
                    bool sameAnimation = (currentTrack.Animation == targetAnimation);
                    if (sameAnimation)
                    {
                        currentTrack.TimeScale = (currentTrack.TimeScale == 0) ? 1f : 0f; // pause/play
                    }
                    else
                    {
                        currentTrack.TimeScale = 1f;
                        animationState.SetAnimation(0, targetAnimation, loop);
                    }
                }

                if (isNewAnimation)
                {
                    currentAnimationEvents.Clear();
                    currentAnimationEventTimes.Clear();
                    foreach (Timeline timeline in targetAnimation.Timelines)
                    {
                        EventTimeline eventTimeline = timeline as EventTimeline;
                        if (eventTimeline != null)
                        {
                            for (int i = 0; i < eventTimeline.Events.Length; i++)
                            {
                                currentAnimationEvents.Add(eventTimeline.Events[i]);
                                currentAnimationEventTimes.Add(eventTimeline.Frames[i]);
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogFormat("The Spine.Animation named '{0}' was not found for this Skeleton.", animationName);
            }

        }

        public void RefreshOnNextUpdate()
        {
            requiresRefresh = true;
        }

        public void ClearAnimationSetupPose(SkeletonAnimation skeletonAnimation)
        {
            if (skeletonAnimation == null)
            {
                Debug.LogWarning("Animation was stopped but preview doesn't exist. It's possible that the Preview Panel is closed.");
            }

            skeletonAnimation.AnimationState.ClearTracks();
            skeletonAnimation.Skeleton.SetToSetupPose();
        }
        static GUIContent tempContent;
        internal static GUIContent TempContent(string text, Texture2D image = null, string tooltip = null)
        {
            if (tempContent == null) tempContent = new GUIContent();
            tempContent.text = text;
            tempContent.image = image;
            tempContent.tooltip = tooltip;
            return tempContent;
        }
    }

}
#endif


