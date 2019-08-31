using Unity.Entities;
using Unity.Tiny.Core2D;
using Unity.Tiny.HitBox2D;
using RigidBodySystems;
using Unity.Collections;

public class OKCheckSystem : ComponentSystem
{
    EntityQueryDesc GlassDesc;
    EntityQuery GlassQuery;

    EntityQueryDesc CheckLineDesc;
    EntityQuery CheckLineQuery;

    EntityQueryDesc OkAreaDesc;
    EntityQuery OkAreaQuery;

    EntityQueryDesc ScoreDesc;
    EntityQuery ScoreQuery;

    Unity.Mathematics.Random RandomData;

    protected override void OnCreate()
    {
        /*ECS�ɂ����āA�N�G���̍쐬��OnCreate�ōs���̂���΂ƂȂ��Ă��܂�*/

        GlassDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(GlassTag), typeof(RigidBody), typeof(GlassComponent),typeof(Translation) },
        };

        CheckLineDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] {typeof(CheckLineTag) },
        };

        OkAreaDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(OKAreaTag) },
        };

        ScoreDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(ScoreComponent) },
        };


        GlassQuery = GetEntityQuery(GlassDesc);
        CheckLineQuery = GetEntityQuery(CheckLineDesc);
        OkAreaQuery = GetEntityQuery(OkAreaDesc);
        ScoreQuery = GetEntityQuery(ScoreDesc);

        RandomData = new Unity.Mathematics.Random(3158456);
    }

    protected override void OnUpdate()
    {
        NativeArray<Entity> OkArea = OkAreaQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<Entity> CheckLine = CheckLineQuery.ToEntityArray(Allocator.TempJob);

        Entities.With(GlassQuery).ForEach((Entity ThisEntity,ref GlassComponent GlassData,ref RigidBody RigidData,ref Translation Trans)=>
        {
            if(!GlassData.Active||GlassData.EndTag)
            {
                return;
            }

            if(RigidData.Velocity.x!=0)
            {
                return;
            }

            var HitEntity = EntityManager.GetBuffer<HitBoxOverlap>(CheckLine[0]);

            bool HitResultFlag = false;


            for (int k = 0; k < HitEntity.Length; k++)
            {
                //�T�[�`�Ώۂ�Entity���܂܂�Ă����ꍇ�A�������Ă��邱�ƂɂȂ�B
                if (OkArea[0]== HitEntity[k].otherEntity)
                {
                    //���������ꍇ�A��x�t���O��True�ɂ���Break�G
                    HitResultFlag = true;
                    break;
                }
            }

            if(HitResultFlag==true)
            {
                //����
                Trans.Value.x = 0;
                GlassData.Active = false;
                ScoreUp();
                RandomSet();
            }
            else
            {
                //���s
                GlassData.EndTag = true;
            }
        });

        OkArea.Dispose();
        CheckLine.Dispose();
    }

    private void ScoreUp()
    {
        Entities.With(ScoreQuery).ForEach((Entity ThisEntity, ref ScoreComponent ScoreData) =>
        {
            ScoreData.Score += 1;
            RandomData.InitState((uint)(ScoreData.Score*RandomData.NextInt(0,1000)) );
        });
    }

    private void RandomSet()
    {
        Entities.With(OkAreaQuery).ForEach((Entity ThisEntity,ref Translation Trans ,ref NonUniformScale Scale ,ref OKAreaTag AreaData) =>
        {
            Trans.Value.x = RandomData.NextFloat(AreaData.MinPos,AreaData.MaxPos);

            Scale.Value.x = RandomData.NextFloat(AreaData.MinPos, AreaData.MaxPos);
        });
    }
}