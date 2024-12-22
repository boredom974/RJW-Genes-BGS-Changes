using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RJW_BGS
{
    /// <summary>
    /// This Patch is applied to change the normal pregnancy to add animal-inheritance. 
    /// If the settings allow animal gene inheritance, 
    /// the genes are determined and "simply added". 
    /// </summary>
    [HarmonyPatch(typeof(PregnancyUtility), "GetInheritedGeneSet", new Type[] 
    { 
        typeof(Pawn), 
        typeof(Pawn),
        //typeof(bool)
    }
    )]
    public static class Patch_Vanilla_PregnancyUtility
    {
        [HarmonyPriority(int.MaxValue)]
        [HarmonyPrefix]
        public static void VanillaGeneInheritance(ref Pawn father, ref Pawn mother)
        {
            if (!RJW_BGSSettings.rjw_bgs_enabled || !RJW_BGSSettings.rjw_bgs_vanilla_inheritance)
            {
                return;
            }

            if (mother.RaceProps.Humanlike != father.RaceProps.Humanlike)
            {
                Pawn animalParent = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist); //can make random xenotype so needs to be stripped of genes. I dont wanna deal with PawnGenerationRequests right now.
                animalParent.genes.SetXenotype(XenotypeDefOf.Baseliner);
                for (int i = animalParent.genes.Endogenes.Count - 1; i >= 0; i--)
                {
                    animalParent.genes.RemoveGene(animalParent.genes.Endogenes[i]);
                }

                if (!father.RaceProps.Humanlike)
                {
                    RJW_Genes.ModLog.Debug($"Father was found to be animal - looking up genes for {father.Name} and applying them to dummy pawn in his place.");
                    InheritanceUtility.AddGenes(animalParent, InheritanceUtility.SelectAllGenes(father));
                    father = animalParent;
                }
                else
                {
                    RJW_Genes.ModLog.Debug($"Mother was found to be animal - looking up genes for {mother.Name} and applying them to dummy pawn in her place.");
                    InheritanceUtility.AddGenes(animalParent, InheritanceUtility.SelectAllGenes(mother));
                    mother = animalParent;
                }
            }
        }
        
        [HarmonyPostfix]
        public static void AnimalInheritedGenes(Pawn father, Pawn mother, ref GeneSet __result)
        {
            if (!RJW_BGSSettings.rjw_bgs_enabled)
            {
                return;
            }
            List<GeneDef> genes = InheritanceUtility.AnimalInheritedGenes(father, mother);
            if (genes.Any())
            {
                RJW_Genes.ModLog.Debug($"Adding {(genes.Count)} Genes from an Animal-Pregnancy between {father} and {mother}");
                foreach (GeneDef gene in genes)
                {
                    __result.AddGene(gene);
                }
            } else
            {
                RJW_Genes.ModLog.Debug($"Tried to add Genes from Animal-Pregnancy between {father} and {mother} but didn't find any");
            }
        }
    }
}
