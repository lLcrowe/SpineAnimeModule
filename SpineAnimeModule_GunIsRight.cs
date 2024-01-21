using UnityEngine;

namespace lLCroweTool.AnimeSystem.Spine
{
    public class SpineAnimeModule_GunIsRight : SpineAnimeModule_FuncBase
    {
        //어트리뷰트를 만들어서 팝업으로 처리예정
        public string attackmentNameID;

        public override void InitSpineData()
        {
            spineAttachmentInfoBook.ActionAttackment(attackmentNameID);
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