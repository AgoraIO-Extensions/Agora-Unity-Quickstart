using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace io.agora.rtm.demo
{
    public class ListItem<T> : MonoBehaviour
    {

        public virtual void DestroySelf()
        {
            Destroy(this.gameObject);
        }

        public virtual T GetDataSource()
        {
            return default(T);
        }

        public virtual void SetDataSource(T dataSource)
        {

        }

        public override string ToString()
        {
            return "baseListItem";
        }
    }
}
