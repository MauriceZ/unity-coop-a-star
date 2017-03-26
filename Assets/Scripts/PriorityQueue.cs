using System.Collections;
using System.Collections.Generic;

public class PriorityQueue<T1> {
  private class QueueNode<T2> {
    public T2 data;
    public float priority;

    public QueueNode(T2 data, float priority) {
      this.data = data;
      this.priority = priority;
    }
  }

  List<QueueNode<T1>> list;

  public PriorityQueue() {
    this.list = new List<QueueNode<T1>>();
  }

  public void Enqueue(T1 data, float priority) {
    list.Add(new QueueNode<T1>(data, priority));
    relocateNode(list.Count - 1);
  }

  public T1 Dequeue() {
    if (list.Count == 0)
      throw new System.IndexOutOfRangeException("PriorityQueue is empty");

    var targetNode = list[0];
    swap(0, list.Count - 1);
    list.RemoveAt(list.Count - 1);
    relocateNode(0);

    return targetNode.data;
  }

  public T1 Peek() {
    if (list.Count == 0)
      throw new System.IndexOutOfRangeException("PriorityQueue is empty");
    return list[0].data;
  }

  public int Count() {
    return list.Count;
  }

  public bool IsEmpty() {
    return list.Count == 0;
  }

  private void relocateNode(int nodeIndex) {
    if (list.Count <= 1)
      return;

    var node = list[nodeIndex];
    var children = getChildrenNode(nodeIndex);

    if (nodeIndex > 0 && getParentNode(nodeIndex).priority > node.priority) {

      var parentIndex = nodeIndex / 2;
      swap(nodeIndex, parentIndex);
      relocateNode(parentIndex);

    } else if (children.Length > 0) {
      var isValid = true;

      foreach (var childNode in children) {
        if (childNode.priority < node.priority)
          isValid = false;
      }

      if (!isValid) {

        var smallerChildIndex = 2 * nodeIndex;

        if (children.Length == 2 && children[0].priority > children[1].priority)
          smallerChildIndex++;

        swap(nodeIndex, smallerChildIndex);
        relocateNode(smallerChildIndex);

      }
    }
  }

  private void swap(int i1, int i2) {
    if (list.Count <= 1)
      return;

    var temp = list[i1];
    list[i1] = list[i2];
    list[i2] = temp;
  }

  private QueueNode<T1> getParentNode(int childIndex) {
    return childIndex == 0 ? null : list[childIndex/2];
  }

  private QueueNode<T1>[] getChildrenNode(int parentIndex) {
    if (2 * parentIndex > list.Count - 1) {
      return new QueueNode<T1>[0]; // 0 children
    } else if (2 * parentIndex + 1 > list.Count - 1) {
      return new QueueNode<T1>[] { list[2 * parentIndex] }; // 1 child
    } else {
      return new QueueNode<T1>[] { list[2 * parentIndex], list[2 * parentIndex + 1] }; // 2 children
    }
  }
}
