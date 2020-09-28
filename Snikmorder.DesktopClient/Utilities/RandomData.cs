using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snikmorder.DesktopClient.Utilities
{
    public static class RandomData
    {
        static Queue<string> Names = new Queue<string>(new List<string>
        {
            "Sannah Lindgren", 
            "Gina Vatland", 
            "Liva Erdal", 
            "Benedikte Nyheim", 
            "Elli Simensen", 
            "Ana Nesheim", 
            "Benedicte Jansen", 
            "Ingelin Kvalsvik", 
            "Villemo Brenne", 
            "Juliane Lyng", 
            "Elsa Salomonsen", 
            "Selma Bergan", 
            "Lilje Evjen", 
            "Malin Håvik", 
            "Alicja Roald", 
            "Gabija Øye", 
            "Ylva Abelsen", 
            "Oline Fredriksen", 
            "Eir Isaksen", 
            "Sophie Anderssen", 
            "Heidi Haug", 
            "Lykke Helland", 
            "Alma Pham", 
            "Line Hjelle", 
            "Elisa Thorbjørnsen", 
            "Johann Kittilsen", 
            "Leon Ekeberg", 
            "Odd Holden", 
            "Sean Olufsen", 
            "Emanuel Hofstad", 
            "Olai Torkildsen", 
            "Leif Sundet", 
            "Joakim Lothe", 
            "Elling Våge", 
            "Nicholas Kleven", 
            "Lasse Aakre", 
            "Steffen Bruun", 
            "Tord Jacobsen", 
            "Vincent Lange", 
            "Joar Mathiesen", 
            "Jørgen Vold", 
            "Ryan Aslaksen", 
            "Conrad Ahmad", 
            "Ivan Norum", 
            "Arijus Krogstad", 
            "David Kjær", 
            "Mio Sæle", 
            "Nils Bjerkan", 
            "Niklas Berland", 
            "Syver Refsnes", 

        }.OrderBy(x=>Guid.NewGuid()));

        public static string GetRandomName()
        {
            return Names.Dequeue();
        }

        
    }
}
