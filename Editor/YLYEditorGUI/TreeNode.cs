/*
折叠树节点控件（编辑器模式下使用）
author：雨Lu尧
email：cantry100@163.com
blog：http://www.hiwrz.com
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace YLYEditorGUI {
	public class TreeNode {
		public static float nodeDefaultMarginX = 20f;
		public static float nodeDefaultWidth = 100f;
		public static float nodeDefaultWidthObject = 180f;
		public static float nodeDefaultHeight = 20f;

		public bool isShow = true;
		public bool isRoot = false;
		public bool isDestroy = false;
		public string title = null;
		public string subTitle = null;
		public UnityEngine.Object assetObject = null;
		public List<TreeNode> children = new List<TreeNode>();
		public TreeNode parent = null;
		public Rect position = new Rect(0, 0, nodeDefaultWidth, nodeDefaultHeight);
		public Vector2 recursiveSize = new Vector2(nodeDefaultWidth, nodeDefaultHeight);
		public float marginX = nodeDefaultMarginX;
		public GUIStyle subTitleGuiStyle = null;

		public TreeNode(string title, UnityEngine.Object assetObject = null){
			this.title = title;
			this.assetObject = assetObject;

			if (title == null) {
				position.width = nodeDefaultWidthObject;
			}
		}

		//外部调用这个函数刷新gui
		public void OnGUI(){
			if (title == null) {
				isShow = EditorGUI.Foldout (new Rect (position.x, position.y, 20, position.height), isShow, "");
				EditorGUI.ObjectField (new Rect (position.x + 20, position.y, 160, position.height), assetObject, typeof(UnityEngine.Object));
				if (subTitle != null) {
					EditorGUI.LabelField (new Rect (position.x + 186, position.y, position.width, position.height), subTitle, subTitleGuiStyle);
				}
			} else {
				isShow = EditorGUI.Foldout (position, isShow, title);
				if (subTitle != null) {
					EditorGUI.LabelField (new Rect (position.x + position.width + 5, position.y, position.width, position.height), subTitle, subTitleGuiStyle);
				}
			}

			if (isShow) {
				float childY = position.y + position.height;
				for (int i = 0; i < children.Count; i++) {
					if (children [i] != null) {
						children [i].position.y = childY;
						children [i].OnGUI ();
						childY = childY + children [i].recursiveSize.y;
					}
				}
			}

			CalcRecursiveSize();
		}

		public int GetShowChildNum(){
			int num = 0;
			if(isShow){
				num++;
			}

			for(int i=0; i<children.Count; i++){
				if(children[i] != null && children[i].IsShowRecursively()){
					num++;
				}
			}

			return num;
		}

		public void SetSize(float width, float height){
			position.width = width;
			position.height = height;
		}

		public void SetMarginX(float marginX){
			this.marginX = marginX;

			for (int i = 0; i < children.Count; i++) {
				if (children [i] != null) {
					children [i].SetPositionX(position.x + this.marginX);
				}
			}
		}

		public void SetSubTitle(string subTitle, Color titleColor){
			this.subTitle = subTitle;

			if(subTitle != null){
				if (subTitleGuiStyle == null) {
					subTitleGuiStyle = new GUIStyle ();
					subTitleGuiStyle.fontSize = 11;
					subTitleGuiStyle.alignment = TextAnchor.MiddleLeft;
				}
				subTitleGuiStyle.normal.textColor = titleColor;
			}
		}

		//计算宽高（包含子节点也一起计算）
		public void CalcRecursiveSize(){
			recursiveSize.x = position.width;
			recursiveSize.y = position.height;

			if (isShow) {
				float childMaxWidth = 0f;
				for (int i = 0; i < children.Count; i++) {
					if (children [i] != null) {
						childMaxWidth = Math.Max(childMaxWidth, children [i].recursiveSize.x);
						recursiveSize.y = recursiveSize.y + children [i].recursiveSize.y;
					}
				}

				//这里只按照所有树节点都是同样的宽度来计算的
				if(childMaxWidth > 0f){
					recursiveSize.x = childMaxWidth + Math.Abs(marginX);
				}
			}
		}

		public void SetPositionX(float x){
			position.x = x;

			for (int i = 0; i < children.Count; i++) {
				if (children [i] != null) {
					children [i].SetPositionX(position.x + marginX);
				}
			}
		}

		public bool IsShowRecursively(){
			if(!isShow){
				return false;
			}

			if(parent == null){
				return isShow;
			}

			return parent.IsShowRecursively();
		}

		public void AddChild(TreeNode node){
			if(node == null){
				return;
			}

			if (node == this) {
				return;
			}

			if (node.isDestroy) {
				return;
			}

			if(node.parent == this){
				return;
			}

			node.SetParent(null);
			children.Add(node);
			node.parent = this;
			node.SetPositionX(position.x + marginX);
		}

		public void RemoveChild(TreeNode node){
			if(node == null){
				return;
			}

			int index = -1;
			for(int i=0; i<children.Count; i++){
				if(node == children[i]){
					index = i;
					break;
				}
			}

			if(index != -1){
				children[index].parent = null;
				children.RemoveAt(index);
			}
		}

		public void RemoveAllChild(){
			for(int i=0; i<children.Count; i++){
				if(children[i] != null){
					children[i].parent = null;
				}
			}
			children.Clear();
		}

		public void DestroyChild(TreeNode node){
			if(node == null){
				return;
			}

			int index = -1;
			for(int i=0; i<children.Count; i++){
				if(node == children[i]){
					index = i;
					break;
				}
			}

			if(index != -1){
				children[index].Destroy();
			}
		}

		public void DestroyAllChild(){
			for(int i=0; i<children.Count; i++){
				if(children[i] != null){
					children[i].Destroy();
				}
			}
			children.Clear();
		}

		public void SetParent(TreeNode node){
			if(node == null){
				if(parent != null){
					parent.RemoveChild(this);
					parent = null;
				}
				return;
			}

			if (node == this) {
				return;
			}

			if (node.isDestroy) {
				return;
			}

			if(node != parent){
				if(parent != null){
					parent.RemoveChild(this);
					parent = null;
				}
				node.AddChild(this);
			}
		}

		//克隆
		public TreeNode Clone(){
			TreeNode cloneNode = new TreeNode (title, assetObject);
			cloneNode.SetSubTitle(subTitle, subTitleGuiStyle != null ? subTitleGuiStyle.normal.textColor : Color.green);
			cloneNode.SetParent(parent);
			for(int i=0; i<children.Count; i++){
				if(children[i] != null){
					cloneNode.AddChild(children[i].Clone());
				}
			}

			return cloneNode;
		}

		//销毁
		public void Destroy(){
			SetParent(null);
			assetObject = null;
			isDestroy = true;

			for(int i=0; i<children.Count; i++){
				if(children[i] != null){
					children[i].Destroy();
				}
			}
			children.Clear();
		}
	}
}