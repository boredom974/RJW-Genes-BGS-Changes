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
    [HarmonyPatch(typeof(PregnancyUtility))]
    public static class Patch_Vanilla_PregnancyUtility
    {
        [HarmonyPatch("GetInheritedGenes", new Type[] {typeof(Pawn), typeof(Pawn), typeof(Boolean)}, new ArgumentType[] {ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out}), HarmonyPriority(int.MaxValue), HarmonyPrefix]
        public static void VanillaGenesInheritance(ref Pawn father, ref Pawn mother)
        {
            if (RJW_BGSSettings.rjw_bgs_enabled && RJW_BGSSettings.rjw_bgs_vanilla_inheritance && mother.RaceProps.Humanlike != father.RaceProps.Humanlike)
            {
                (Pawn animalParent, bool fatherIsAnimal) = InheritanceUtility.CreateAnimalGeneDummy(father, mother);
                if (fatherIsAnimal)
                    father = animalParent;
                else
                    mother = animalParent;
            }
        }
        
        [HarmonyPatch("GetInheritedGeneSet", new Type[] {typeof(Pawn), typeof(Pawn)}), HarmonyPostfix]
        public static void AnimalInheritedGenes(Pawn father, Pawn mother, ref GeneSet __result)
        {
            if (!RJW_BGSSettings.rjw_bgs_enabled || RJW_BGSSettings.rjw_bgs_vanilla_inheritance)
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
