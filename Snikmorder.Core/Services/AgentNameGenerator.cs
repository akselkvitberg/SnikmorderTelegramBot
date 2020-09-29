using System;
using System.Collections.Generic;
using System.Linq;

namespace Snikmorder.Core.Services
{
    internal static class AgentNameGenerator
    {

        static Queue<string> AgentNames = new Queue<string>(new List<string>
        {
            "Fiskeben",
            "Langpanne",
            "Kokosnøtt",
            "Perry",
            "Havrefras",
            "Fjellskred",
            "Croissant",
            "Pepperbiff",
            "Lårhals",
            "Solsikke",
            "Grantre",
            "Trehytte",
            "Gjerv",
            "Nesehorn-horn",
            "Ryggmassasje",
            "Laksefilet",
            "Maragarin",
            "Kornåker",
            "Mineralvann",
            "Kostfiber",
            "Griffel",
            "Fruktsalat",
            "Putelaken",
            "Hjørneskuff",
            "Neseblod",
            "Bjørnehi",
            "Hodepine",
            "Algebra",
            "Lasagne",
            "Dørhåndtak",
            "Kaffemaskin",
            "Takvifte",
            "Klokketårn",
            "Gressgutt",
            "Ekkorn",
            "xX_DragonSlayer_Xx",
            "Vannkopp",
            "Salathode",
            "Kneip",
            "Vagle",
            "Knipe",
            "Rødkål",
            "Skogsbær",
            "Risgraut",
            "Albatross",
            "Sleipner",
            "Bismarck",
            "Brennmanet",
            "Maiskolbe",
            "Ullgenser",
            "Trombone",
            "Fingerneil",
            "Flosshatt",
            "Spruteflaske",
            "Eventyrskute",
            "Garderobemann",
            "Tonefall",
            "Stemmebånd",
            "Leggbeskytter",
            "Ankelsmerte",
            "Sabeltann",
            "Dagligvare",
            "Spaghetti",
            "Papptallerken",
            "Sjøfisk",
            "Løvblåser",
            "Sitteunderlag",
            "Gresshopper"
        }.OrderBy(x => Guid.NewGuid()));

        public static string GetAgentName()
        {
            return AgentNames.Dequeue();
        }
    }
}