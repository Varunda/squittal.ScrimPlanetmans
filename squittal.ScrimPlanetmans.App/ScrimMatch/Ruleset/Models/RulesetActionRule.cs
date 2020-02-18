﻿using System.ComponentModel.DataAnnotations;

namespace squittal.ScrimPlanetmans.ScrimMatch.Models
{
    public class RulesetActionRule
    {
        [Required]
        public int RulesetId { get; set; }

        [Required]
        public ScrimActionType ScrimActionType { get; set; }

        public int Points { get; set; }


        public Ruleset Ruleset { get; set; }

        //public ScrimAction ActionModel { get; set; }
    }
}