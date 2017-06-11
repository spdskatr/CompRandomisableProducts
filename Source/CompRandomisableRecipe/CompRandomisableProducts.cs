using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace CompRandomisableProducts
{
    public class DefLowUpperTripleWeighted : DefLowUpperTriple
    {
        public float weight = 1f;
    }
    public class PickOneOutOfRangeWeighted : DefLowUpperTriple
    {
        public List<DefLowUpperTripleWeighted> weightedOptions;
        public override ThingCountClass Randomise()
        {
            if (weightedOptions?.Any() ?? false)
            {
                return weightedOptions.RandomElementByWeight(t => t.weight).Randomise();
            }
            throw new NullReferenceException("CompRandomisableProducts.PickOneOutOfRange but there are no options.");
        }
    }
    public class PickOneOutOfRange : DefLowUpperTriple
    {
        public List<DefLowUpperTriple> options;
        public override ThingCountClass Randomise()
        {
            if (options?.Any() ?? false)
            {
                return options.RandomElement().Randomise();
            }
            throw new NullReferenceException("CompRandomisableProducts.PickOneOutOfRange but there are no options.");
        }
    }
    public class DefLowUpperTriple
    {
        public ThingDef def;
        public int lowerLimit;
        public int upperLimit;
        public virtual ThingCountClass Randomise()
        {
            int amount = GenMath.RoundRandom(lowerLimit + Rand.Value * (upperLimit - lowerLimit));
            return new ThingCountClass(def, amount);
        }
    }
    public class CompProperties_RandomisableProducts : CompProperties
    {
        public List<DefLowUpperTriple> limitsForProducts = new List<DefLowUpperTriple>();

        public CompProperties_RandomisableProducts()
        {
            compClass = typeof(CompRandomisableProducts);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            if (base.ConfigErrors(parentDef) != null)
                foreach (var s in base.ConfigErrors(parentDef))
                    yield return s;
            if (parentDef.butcherProducts == null)
            {
                yield return "ThingDef " + parentDef.defName + " has CompProperties_RandomisableProducts, when it has no butcher products. Either remove CompProperties_RandomisableProducts or add butcher products.";
                yield break;
            }
            for (int i = 0; i < limitsForProducts.Count; i++)
            {
                var limit = limitsForProducts[i];
                if (limit.lowerLimit > limit.upperLimit)
                    yield return "CompProperties_RandomisableProducts in " + parentDef.defName + ": lowerLimit of " + limit.def + "is greater than upperLimit.";
                
            }
        }
    }
    public class CompRandomisableProducts : ThingComp
    {
        static bool first = false;
        public CompProperties_RandomisableProducts Props => (CompProperties_RandomisableProducts)props;
        public override void PostExposeData()
        {
            base.PostExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit) Randomise();
        }
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            Randomise();
        }
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            if (!first)
            {
                Randomise();
            }
        }

        public void Randomise()
        {
            if (parent.def.butcherProducts != null)
            {
                parent.def.butcherProducts.Clear();
                foreach (var item in Props.limitsForProducts)
                {
                    parent.def.butcherProducts.Add(item.Randomise());
                }
            }
        }
    }
}
