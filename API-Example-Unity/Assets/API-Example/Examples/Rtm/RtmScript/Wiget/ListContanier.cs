using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace io.agora.rtm.demo
{
    public class ListContanier<T> : MonoBehaviour
    {
        [SerializeField]
        public GameObject Prefab;
        [SerializeField]
        public GameObject Container;


        public void AddNewNodeWitoutReturn()
        {
            GameObject gameObject = Instantiate(this.Prefab, this.Container.transform);
        }

        public ListItem<T> AddNewNode()
        {
            GameObject gameObject = Instantiate(this.Prefab, this.Container.transform);
            return gameObject.GetComponent<ListItem<T>>();
        }

        public void ClearAllNode()
        {
            int childCount = this.Container.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Destroy(this.Container.transform.GetChild(i).gameObject);
            }
        }

        public T[] GetDataSource()
        {
            ListItem<T>[] comps = this.GetComponentsInChildren<ListItem<T>>();
            T[] source = new T[comps.Length];
            int i = 0;
            foreach (var com in comps)
            {
                source[i] = com.GetDataSource();
                i++;
            }
            return source;
        }

        public void SetDataSource(T[] dataSource)
        {
            this.ClearAllNode();
            foreach (var data in dataSource)
            {
                ListItem<T> t = this.AddNewNode();
                t.SetDataSource(data);
            }
        }

        public override string ToString()
        {
            string str = "";

            ListItem<T>[] comps = this.GetComponentsInChildren<ListItem<T>>();
            foreach (var com in comps)
            {
                str += ("------" + com.ToString() + "\n");
            }
            return str;
        }

    }
}