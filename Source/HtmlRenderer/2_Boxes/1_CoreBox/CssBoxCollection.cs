﻿//2014 BSD,WinterDev


using System;
using System.Collections.Generic;

namespace HtmlRenderer.Boxes
{

    class CssBoxCollection
    {

        LinkedList<CssBox> _boxes = new LinkedList<CssBox>();
        CssBox owner;
        public CssBoxCollection(CssBox owner)
        {
            this.owner = owner;
        }
        public IEnumerable<CssBox> GetChildBoxIter()
        {
            var cNode = _boxes.First;
            while (cNode != null)
            {
                yield return cNode.Value;
                cNode = cNode.Next;
            }
        }
        public IEnumerable<CssBox> GetChildBoxBackwardIter()
        {
            var cNode = _boxes.Last;
            while (cNode != null)
            {
                yield return cNode.Value;
                cNode = cNode.Previous;
            }
        }
        public void AddChild(CssBox box)
        {

#if DEBUG
            if (this.owner == box)
            {
                throw new NotSupportedException();
            }
#endif
            CssBox.UnsafeSetNodes(box, this.owner, _boxes.AddLast(box));
        }
        public void InsertBefore(CssBox beforeBox, CssBox box)
        {
            var beforeLinkedNode = CssBox.UnsafeGetLinkedNode(beforeBox);
            CssBox.UnsafeSetNodes(box, this.owner,
                this._boxes.AddBefore(beforeLinkedNode, box));
        }
        public int Count
        {
            get
            {
                return this._boxes.Count;
            }
        }
        public void Remove(CssBox box)
        {
            var linkedNode = CssBox.UnsafeGetLinkedNode(box);
            this._boxes.Remove(linkedNode);
            CssBox.UnsafeSetNodes(box, null, null);
        }
        public CssBox GetFirstChild()
        {
            return this._boxes.First.Value;
        }
        public LinkedListNode<CssBox> GetFirstLinkedNode()
        {
            return this._boxes.First;
        }
        public LinkedListNode<CssBox> GetLastLinkedNode()
        {
            return this._boxes.Last;
        }

        public CssBox GetLastChild()
        {
            return this._boxes.Last.Value;
        }
        public IEnumerator<CssBox> GetEnumerator()
        {
            return this._boxes.GetEnumerator();
        }



#if DEBUG
        internal LinkedListNode<CssBox> dbugGetNodeAtIndex(int index)
        {
            //for compat with old version
            switch (index)
            {
                //hint
                case 0:
                    {
                        return this._boxes.First;//0
                    }
                case 1:
                    {
                        return this._boxes.First.Next;//0,1

                    }
                case 2:
                    {
                        return this._boxes.First.Next.Next;//0,1,2 
                    }
                case 3:
                    {
                        return this._boxes.First.Next.Next.Next;//0,1,2,3
                    }
                default:
                    {

                        int j = this._boxes.Count;
                        var cnode = this._boxes.First;
                        for (int i = 0; i < j; ++i)
                        {
                            if (i == index)
                            {
                                return cnode;

                            }
                            else if (i > index)
                            {
                                return null;
                            }
                            cnode = cnode.Next;
                        }
                        return null;//not found                         
                    }
            }
        }



        public void dbugChangeSiblingIndex(CssBox box, int newIndex)
        {   
            //for compat with old version
            //find target linked node 
            LinkedListNode<CssBox> foundNode = this.dbugGetNodeAtIndex(newIndex);
            //1. remove from current box
            this.Remove(box);
            //2. 
            CssBox.UnsafeSetNodes(box, this.owner, this._boxes.AddBefore(foundNode, box));
        }
#endif
    }


}