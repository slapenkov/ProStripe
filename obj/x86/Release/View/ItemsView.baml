   M S B A M L   `   `   `  ����   	ProStripe    ProStripe.View.ItemsView   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     
�������  � 	Resources   �� 
TargetType �� Property �� Value �� Triggers P� Property P� Value��%p  '��       '�� -     '�� m     '�� #    �� " ��.�-   ��  � ��$ Stretch=��� " ��.���  � ��$ Stretch=���  � ��$
 Left=��� " ��.���  � ��
 ���� ��  � ��
 ���� ��  � ��$ 0q���  � ��$ -4q���  � |� .   P�  � {� .  . ���  �@J�Opacity$	 0.3Q��� " ��.���  � ��$
 None=�.��� -    �Name$ Layout�� � ColumnDefinitions �� $��* �	 � RowDefinitions	 �� $
��Auto ��� $��* �.+�]� -   
 ]�Name$
 pathNavigate����. .+��� -    ��Name$
 Back��$��5,0,0,0q����� *commandBack+.���� $"��/Images/Circle-left-icon.png��$��Center=�$��Center=��� -   $ Forward��$��5,0,0,0q����� *commandForward+.���� $#��/Images/Circle-right-icon.png��$��Center=�$��Center=��� -   $ Up��$��5,0,0,0q�$��Right=����� *	commandUp+.���� $ ��/Images/Circle-up-icon.png��$��Center=�$��Center=��� -    ��Name$ textRoot��$��23�� �� 
IsEditable . $��5,0,0,0q� �� Text ��  �� Path$
 Root� �� UpdateSourceTrigger$ Explicit=��� -    ��Name$ fileListView��$��1��$��0��$
��Auto=�$��Stretch=����� $ Children� �� NotifyOnTargetUpdated .  �� View �� .P���  �� Width$
 Auto��.���� -	   
 � Tag$
 Name��$�� Name ��$��	0,0,120,0q� �� CellTemplate �� .R���  �� Orientation$ 
Horizontal=�$
��Auto��.+��� $
��None=����� *Icon+�� $��5,0,0,0q����� *Name+�� $
 Auto�� �� DisplayMemberBinding �� *
FileType+.���� -
   $ FileType��$�� Type ���� $
 Auto�� �� *LastWriteTime+.���� -   $ LastWriteTime��$�� Last written��