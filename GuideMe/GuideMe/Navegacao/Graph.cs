using GuideMe.TOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace GuideMe.Navegacao
{
    public class GraphDFS
    {
        private Dictionary<int, List<int>> nodos;
        public GraphDFS(EstabelecimentoTagsTO data)
        {
            nodos = new Dictionary<int, List<int>>();
            foreach (var tag in data.Tags)
            {
                foreach (var tagDependente in tag.TagsPai)
                {
                    if(tag.Id!= tagDependente.Id_Tag)
                        AddEdge(tag.Id, tagDependente.Id_Tag);
                    else
                        AddEdge(tag.Id, tagDependente.Id_Tag_Pai);
                }
            }

            
        }
        public List<int> CalcularRota(int startNode, int nodoDesejado)
        {
            List<int> rota = new List<int>();
            HashSet<int> visited = new HashSet<int>();
            bool nodoEncontrado = false;
            DFSRecursive(startNode, visited, nodoDesejado, rota, ref nodoEncontrado);

            if (rota.Count<=0 || rota.Count == 1 && startNode != nodoDesejado)
                return null;
            else
                return rota;
        }

        private void DFSRecursive(int node, HashSet<int> visited, int nodoDesejado, List<int> rota, ref bool nodoEncontrado)
        {
            visited.Add(node);
            Console.Write(node + " ");

            if (nodos.ContainsKey(node) && !nodoEncontrado)
            {
                foreach (var neighbor in nodos[node])
                {
                    if (!nodoEncontrado)
                    {
                        if (!visited.Contains(neighbor) && node != nodoDesejado)
                        {
                            rota.Add(node);
                            DFSRecursive(neighbor, visited, nodoDesejado, rota, ref nodoEncontrado);
                        }
                        else if (node == nodoDesejado)
                        {
                            nodoEncontrado = true;
                            rota.Add(node);
                        }
                    }
                    else
                        break;
                    
                        
                }
            }
        }
        public void AddEdge(int v, int w)
        {
            if (!nodos.ContainsKey(v))
                nodos[v] = new List<int>();
            nodos[v].Add(w);
        }
    }
}

   

