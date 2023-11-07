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
            List<int> rotaAux = new List<int>();
            HashSet<int> visited = new HashSet<int>();
            bool nodoEncontrado = false;
            rota=BFS(startNode, nodoDesejado);
            foreach (var i in rota)
            {
                rotaAux.Add(i);
                if (i == nodoDesejado)
                    break;
            }

            //DFSRecursive(startNode, visited, nodoDesejado, rota, ref nodoEncontrado);
            //DFSHelper(startNode, nodoDesejado, visited, rota);
            
            if (rotaAux.Count<=0 || rotaAux.Count == 1 && startNode != nodoDesejado)
                return null;
            else
                return rotaAux;
        }
        public List<int> BFS(int startNode, int desiredNode)
        {
            var visited = new HashSet<int>();
            var route = new List<int>();

            if (BFSHelper(startNode, desiredNode, visited, route))
                return route;
            else
                return new List<int>(); // No route found
        }

        private bool BFSHelper(int startNode, int desiredNode, HashSet<int> visited, List<int> route)
        {
            Queue<int> queue = new Queue<int>();
            Dictionary<int, int> parentMap = new Dictionary<int, int>();

            visited.Add(startNode);
            queue.Enqueue(startNode);

            while (queue.Count > 0)
            {
                int currentNode = queue.Dequeue();
                route.Add(currentNode);

                if (currentNode == desiredNode)
                {
                    // Reconstruct the route from startNode to desiredNode
                    int node = desiredNode;
                    while (node != startNode)
                    {
                        route.Insert(0, node);
                        node = parentMap[node];
                    }
                    route.Insert(0, startNode);
                    return true;
                }

                if (nodos.ContainsKey(currentNode))
                {
                    foreach (var neighbor in nodos[currentNode])
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            parentMap[neighbor] = currentNode;
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            return false; // No route found
        }
    

    private void DFSRecursive(int node, HashSet<int> visited, int nodoDesejado, List<int> rota, ref bool nodoEncontrado, Stack<int> stack = null)
        {
            /*visitNode(node,visited,stack, nodoDesejado);

            while (stack.Count >= 0)
            {
                var aux = stack.Pop();
                visitNode(aux, visited, stack, nodoDesejado);
            }*/
            visited.Add(node);
            if (nodos.ContainsKey(node) && !nodoEncontrado)
            {
                foreach (var neighbor in nodos[node])
                {
                    if (!nodoEncontrado)
                    {
                        if (!visited.Contains(neighbor) && node != nodoDesejado)
                        {
                            rota.Add(neighbor);
                            DFSRecursive(neighbor, visited, nodoDesejado, rota, ref nodoEncontrado);
                        }
                        else if (node == nodoDesejado)
                        {
                            nodoEncontrado = true;
                            if(!rota.Contains(node))
                                rota.Add(node);
                        }

                        if(!nodoEncontrado && visited.Contains(neighbor) && rota.Contains(neighbor))
                            rota.Remove(neighbor);
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
   

