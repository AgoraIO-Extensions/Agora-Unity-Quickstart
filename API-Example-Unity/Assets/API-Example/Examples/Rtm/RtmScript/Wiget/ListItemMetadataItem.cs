using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtm;
namespace io.agora.rtm.demo
{
    public class ListItemMetadataItem : ListItem<MetadataItem>
    {
        public InputField KeyInput;
        public InputField ValueInput;
        public InputField AuthorUserIdInput;
        public InputField RevisionInput;
        public InputField UpdateTsInput;


        public override MetadataItem GetDataSource()
        {
            MetadataItem item = new MetadataItem();
            item.key = KeyInput.text;
            item.value = ValueInput.text;
            item.authorUserId = AuthorUserIdInput.text;
            item.revision = System.Int64.Parse(RevisionInput.text);
            item.updateTs = System.Int64.Parse(UpdateTsInput.text);
            return item;
        }

        public override string ToString()
        {
            return string.Format("key:{0}, value:{1}, authorUserId:{2}, revision:{3}, updateTs:{4}",
                this.KeyInput.text, this.ValueInput.text, this.AuthorUserIdInput.text, this.RevisionInput.text, this.UpdateTsInput.text);
        }

        public override void SetDataSource(MetadataItem dataSource)
        {
            this.KeyInput.text = dataSource.key;
            this.ValueInput.text = dataSource.value;
            this.AuthorUserIdInput.text = dataSource.authorUserId.ToString();
            this.RevisionInput.text = dataSource.revision.ToString();
            this.UpdateTsInput.text = dataSource.updateTs.ToString();
        }
    }
}

