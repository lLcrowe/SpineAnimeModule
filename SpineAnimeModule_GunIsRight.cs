using UnityEngine;

namespace lLCroweTool.AnimeSystem.Spine
{
    public class SpineAnimeModule_GunIsRight : SpineAnimeModule_FuncBase
    {
        public SpineAttachmentInfo spineAttachmentInfo = new SpineAttachmentInfo();

        public override void InitSpineData()
        {
            spineAttachmentInfo.Init(this);
            spineAttachmentInfo.SetThisAttachment();
        }

        public void ActionWalkAnim(Vector2 direction)
        {
            if (direction == Vector2.zero)
            {
                spineAnimDefineInfoBook.ActionAnim(this, "Idle");
                return;
            }
            spineAnimDefineInfoBook.ActionAnim(this, "Walk");
        }

        public void ActionAttackAnim()
        {
            spineAnimDefineInfoBook.ActionAnim(this, "Attack");
        }

    
    }
}