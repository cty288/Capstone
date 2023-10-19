using System.Collections.Generic;
using _02._Scripts.Runtime.Levels;
using Framework;
using JetBrains.Annotations;
using NUnit.Framework;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Builders;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.Properties.TagProperty;
using Runtime.DataFramework.Properties.TestOnly;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Builders;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities.Collision;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace Tests.Tests_Editor {
    public class TestEnemy {

        internal class TestBasicEnemy : BossEntity<TestBasicEnemy> {
            [field: ES3Serializable]
            public override string EntityName { get; set; } = "TestEnemy2";


            protected override string OnGetDescription(string defaultLocalizationKey) {
                return null;
            }
            public override void OnRecycle() {
            
            }
            protected override ConfigTable GetConfigTable() {
                return ConfigDatas.Singleton.EnemyEntityConfigTable_Test;
            }

            protected override void OnEntityStart(bool isLoadedFromSave) {
                
            }


            protected override void OnEnemyRegisterAdditionalProperties() {
               // RegisterInitialProperty<IVigilianceProperty>(new TestVigiliance());
                RegisterInitialProperty<IAttackRangeProperty>(new TestAttackRange());
                RegisterInitialProperty<TestHashSetProperty>(new TestHashSetProperty());
            }

            protected override void OnInitModifiers(int rarity, int level) {
                
            }

            protected override ICustomProperty[] OnRegisterCustomProperties() {
                return null;
            }
        }
        
        internal class TestFriendlyEntity : AbstractCreature, ICanDealDamage {
            [field: ES3Serializable]
            public override string EntityName { get; set; } = "TestEnemy2";

            protected override ConfigTable GetConfigTable() {
                return ConfigDatas.Singleton.EnemyEntityConfigTable_Test;
            }

            protected override void OnEntityStart(bool isLoadedFromSave) {
                
            }


            protected override string OnGetDescription(string defaultLocalizationKey) {
                return null;
            }
            public override void OnDoRecycle() {
                
            }
            protected override void OnInitModifiers(int rarity) {
                
            }
            public override void OnRecycle() {
            
            }

            public void Test() {
                //Localization.GetJoin()
            }
            
            protected override void OnEntityRegisterAdditionalProperties() {
                base.OnEntityRegisterAdditionalProperties();
               // RegisterInitialProperty(new TestVigiliance());
                RegisterInitialProperty(new TestAttackRange());
            }

            protected override ICustomProperty[] OnRegisterCustomProperties() {
                return null;
            }

            protected override Faction GetDefaultFaction() {
                return Faction.Friendly;
            }

            public void OnKillDamageable(IDamageable damageable) {
                
            }

            public void OnDealDamage(IDamageable damageable, int damage) {
                
            }

            public ICanDealDamageRootEntity RootDamageDealer { get; }
        }
    
        //===============================Start writing your tests here===============================
        [Test]
        public void TestBasicEnemyProperties() {
            ICommonEntityModel model = MainGame_Test.Interface.GetModel<ICommonEntityModel>();


            TestBasicEnemy ent1 = model.GetBuilder<TestBasicEnemy>(2)
                .SetModifier(new PropertyNameInfo(PropertyName.danger), new TestBasicEntityProperty.MyNewDangerModifier()).
                SetProperty(new PropertyNameInfo(PropertyName.health), new HealthInfo(100,100)).
                SetProperty(new PropertyNameInfo(PropertyName.taste), new List<TasteType>() {TasteType.Type1, TasteType.Type2}).
                SetProperty(new PropertyNameInfo(PropertyName.vigiliance), 100.0f). 
                SetProperty(new PropertyNameInfo(PropertyName.attack_range), 200.0f).
                Build();
        
            Assert.AreEqual(200, ent1.GetProperty<IDangerProperty>().RealValue.Value);
            Assert.GreaterOrEqual(1000f, ent1.GetProperty<IHealthProperty>().RealValue.Value.CurrentHealth);
            Assert.AreEqual(TasteType.Type1, ent1.GetProperty<ITasteProperty>().RealValues[0]);
            Assert.AreEqual(100.0f, ent1.GetProperty<IVigilianceProperty>().RealValue.Value);
            Assert.AreEqual(2000.0f, ent1.GetProperty<IAttackRangeProperty>().RealValue.Value);
        
            //another convenient ways
            Assert.AreEqual(200, ent1.GetDanger().Value);
            Assert.GreaterOrEqual(1000f, ent1.GetHealth().Value.CurrentHealth);
            //Assert.AreEqual(TasteType.Type1, ent1.GetTaste()[0]);
            //Assert.AreEqual(1000.0f, ent1.GetVigiliance().Value);
            //Assert.AreEqual(2000.0f, ent1.GetAttackRange().Value);
        
            Debug.Log($"Danger: {ent1.GetProperty<IDangerProperty>().RealValue}");
        }
    
        [Test]
        public void TestBasicEnemyBuilder() {
            IEnemyEntityModel model = MainGame_Test.Interface.GetModel<IEnemyEntityModel>();


            TestBasicEnemy ent1 = model.GetEnemyBuilder<TestBasicEnemy>(2)
                .SetAllBasics(0, new HealthInfo(100, 100), TasteType.Type1, TasteType.Type2)
                .SetProperty(new PropertyNameInfo(PropertyName.vigiliance), 100f, null)
                .SetProperty(new PropertyNameInfo(PropertyName.attack_range), 200f, null)
                .SetDangerModifier(new TestBasicEntityProperty.MyNewDangerModifier())
                .Build();

            Assert.AreEqual(200, ent1.GetProperty<IDangerProperty>().RealValue.Value);
            Assert.GreaterOrEqual(1000f, ent1.GetProperty<IHealthProperty>().RealValue.Value.CurrentHealth);
            Assert.AreEqual(TasteType.Type1, ent1.GetProperty<ITasteProperty>().RealValues[0]);
            Assert.AreEqual(100.0f, ent1.GetProperty<IVigilianceProperty>().RealValue.Value);
            Assert.AreEqual(2000.0f, ent1.GetProperty<IAttackRangeProperty>().RealValue.Value);
        
        }
    
        [Test]
        public void TestEnemyTags() {
            IEnemyEntityModel model = MainGame_Test.Interface.GetModel<IEnemyEntityModel>();


            TestBasicEnemy ent1 = model.GetEnemyBuilder<TestBasicEnemy>(2)
                .FromConfig()
                .Build();
            
            

            ITagProperty tagProperty = ent1.GetTagProperty();
            Assert.AreEqual(2, tagProperty.GetTags().Length);
            Assert.IsTrue(tagProperty.HasTag(TagName.Test_Ice));
            Assert.IsFalse(tagProperty.HasTag(TagName.Test_Wood));
        
            Assert.AreEqual(3, tagProperty.GetTagLevel(TagName.Test_Flame));
        
            int flameLevel = 0;
            tagProperty.HasTag(TagName.Test_Flame, out flameLevel);
            Assert.AreEqual(3, flameLevel);
        
            Assert.IsTrue(tagProperty.HasTagOverLevel(TagName.Test_Flame, 2));
       
        }
        
        [Test]
        public void TestAddEnemyTags() {
            IEnemyEntityModel model = MainGame_Test.Interface.GetModel<IEnemyEntityModel>();


            TestBasicEnemy ent1 = model.GetEnemyBuilder<TestBasicEnemy>(2)
                .FromConfig()
                .AddTag(TagName.Test_Thunder, 100)
                .Build();
            
            

            ITagProperty tagProperty = ent1.GetTagProperty();
            Assert.AreEqual(3, tagProperty.GetTags().Length);
            Assert.IsTrue(tagProperty.HasTag(TagName.Test_Thunder));

            Assert.AreEqual(100, tagProperty.GetTagLevel(TagName.Test_Thunder));

        }
            
        [Test]
        public void TestCreature() {
            IEnemyEntityModel model = MainGame_Test.Interface.GetModel<IEnemyEntityModel>();


            TestBasicEnemy ent1 = model.GetEnemyBuilder<TestBasicEnemy>(2)
                .FromConfig()
                .Build();

            TestFriendlyEntity ent2 = new EntityBuilderFactory()
                .GetBuilder<EnemyBuilder<TestFriendlyEntity>, TestFriendlyEntity>(1)
                .SetHealth(new HealthInfo(200,200))
                .Build();


            Assert.IsTrue(ent1 is ICreature);
            
            Assert.AreEqual(200, ent2.GetCurrentHealth());
            Assert.LessOrEqual(999, ent1.GetCurrentHealth());


            ent1.RegisterOnTakeDamage(OnEnt1TakeDamage);
            ent1.TakeDamage(200, ent2);
            
            void OnEnt1TakeDamage(int damage, int currenthealth, IBelongToFaction damagedealer, [CanBeNull] HitData hitData) {
                Assert.AreEqual(200, damage);
                Assert.AreEqual(899, currenthealth);
                Assert.AreEqual(ent2, damagedealer);
                ent1.UnRegisterOnTakeDamage(OnEnt1TakeDamage);
            }
            
            Assert.AreEqual(899, ent1.GetCurrentHealth());
            
            //when invincible, damage taken will be 0
            ent1.IsInvincible.Value = true;
            ent1.TakeDamage(200, ent2);
            Assert.AreEqual(899, ent1.GetCurrentHealth());
            
            //when the damage dealer has the same faction, damage will not be taken
            ent1.TakeDamage(100, ent1);
            Assert.AreEqual(899, ent1.GetCurrentHealth());
            
            
            ent1.Heal(10000, ent2);
            Assert.AreEqual(1099, ent1.GetCurrentHealth());
        }
        
        
        [Test]
        public void TestHashset() {
            IEnemyEntityModel model = MainGame_Test.Interface.GetModel<IEnemyEntityModel>();


            TestBasicEnemy ent1 = model.GetEnemyBuilder<TestBasicEnemy>(2)
                .FromConfig()
                .Build();

            Assert.IsTrue(ent1.GetProperty<TestHashSetProperty>().RealValues.Contains("Name2"));
            Assert.IsFalse(ent1.GetProperty<TestHashSetProperty>().RealValues.Contains("Name4"));

            ent1.GetProperty<TestHashSetProperty>().RealValues.RegisterOnAdd(OnEnt1HashsetAdd);
            ent1.GetProperty<TestHashSetProperty>().RealValues.AddAndInvoke("Name5");
            
            void OnEnt1HashsetAdd(string obj) {
                Assert.AreEqual(4, ent1.GetProperty<TestHashSetProperty>().RealValues.Count);
                Assert.AreEqual(3, ent1.GetProperty<TestHashSetProperty>().BaseValue.Count);
            }
            Assert.AreEqual(4, ent1.GetProperty<TestHashSetProperty>().RealValues.Count);
            
            
            ES3.Save("test_save_hashset_entity", ent1, "test_save");
            model.RemoveEntity(ent1.UUID);
            
            ent1 = ES3.Load<TestBasicEnemy>("test_save_hashset_entity", "test_save");
            ent1.OnLoadFromSave();
			
            Assert.IsNotNull(ent1);
            
            Assert.IsTrue(ent1.GetProperty<TestHashSetProperty>().RealValues.Contains("Name2"));
            
            Assert.AreEqual(4, ent1.GetProperty<TestHashSetProperty>().RealValues.Count);
            Assert.AreEqual(3, ent1.GetProperty<TestHashSetProperty>().BaseValue.Count);
            
            
            ES3.DeleteKey("test_save_hashset_entity", "test_save");
        }
        
        [Test]
        public void TestGeneralModifier() {
            int res1 = GlobalLevelFormulas.GetGeneralEnemyAbilityModifier<int>(()=>1, ()=>2, false).Invoke(10);
            
            float res2 = GlobalLevelFormulas.GetGeneralEnemyAbilityModifier<float>(()=>1, ()=>3, false).Invoke(5.2f);
            
            Debug.Log("TestGeneralModifier res1: " + res1);
            Debug.Log("TestGeneralModifier res2: " + res2);
            
        }

        


    }
}
