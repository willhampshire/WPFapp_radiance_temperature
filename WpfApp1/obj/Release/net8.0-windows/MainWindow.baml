   M S B A M L   `   `   `  ����   WpfApp1    WpfApp1.MainWindow   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     R NOxyPlot.Wpf, Version=2.1.2.0, Culture=neutral, PublicKeyToken=75e952ba404cdbb0Y UOxyPlot.Wpf.Shared, Version=2.1.2.0, Culture=neutral, PublicKeyToken=75e952ba404cdbb0"oxyhttp://oxyplot.org/wpf     � Title$&   Radiance - Temperature generator��$	��700��$	��820��.��� .+��� -    ��Name$ MainTabControl��.@��� $��Input��.���  � ColumnDefinitions �� $	��250 ��� $	��300 ��� $��* � � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��* �.+�� $��2��$��0��$��3�� �� $
��Auto ��� $��* �.+��� -   $��2��$��0��$	��200��$��10q�$��Validate parameters���� -   $��ValidateInfoText��$��10q�$��2��$��1��$*��$Validation results will appear here.��$��WrapWithOverflow=�$��Center=�� $��0��$��0��.+��� -   $��i��$��20��$��Right=�$��10q��� -   $��Upload filter csv��$
��Left=�$	��200��$��10q�� $��0��$��1��$��2��$	��250�� �� $
��Auto ��� $��* �.+��� $��Filter preview��$��Center=�$��Center=�$��14(��� $��1��$��Visible=�$��Visible=�$��10q�.��&   System.Windows.Controls.DataGrid  -     Name$ PreviewDataGrid��   AutoGenerateColumns$
 True��   AlternatingRowBackground
 ������ $��0��$��4��$��10q� �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��* �.+�� $��1��$��	20,8,20,0q� �� $��50 ��� $��* �.+��� $
��Name��$��Center=�$
��Left=��� -   $��NewFilterName��$	��100��$��Right=�$��Center=�$��1���� $��Database��$��16(�$
��Bold"�$��Center=�$��Center=�$��0����  �� Orientation$ 
Horizontal=�$��Center=�$��10,5, 10, 0q�$��2��.+��� -   $	��Add��$��75��$��5q��� -	   $��Delete��$��75��$��5q��� $ 
Horizontal=�$��Center=�$
��10,0q�$��3��.+��� -
   $��	Duplicate��$��75��$��5q��� -   $��Rename��$��75��$��5q��� -   $��4��$��Activate filter��$	��130��$��5q�$��Center=��� -   $��FiltersDatabaseListBox��$
��10,5q�$��5���� $��1��$��0��$��
Parameters��$��16(�$
��Bold"�$��Center=�$��Center=��� $��1��$��1��$��10q�$	��Top=�.+��� $��Radiometric parameters��$��Center=�$��Center=�$��14(�� $
��0,10q� �� $	��100 ��� $��* �.+��� $��Low limit λ [μm]��$��Center=��� -   $��
LowlimText��$	��100��$��Right=�$��Center=�$��1��� $
��0,10q� �� $	��100 ��� $��* �.+��� $��High limit λ [μm]��$��Center=��� -   $��HighLimText��$	��100��$��Right=�$��Center=�$��1���� $��1��$��2��$��10q�$	��Top=�.+��� $��Interpolation parameters��$��Center=�$��Center=�$��14(�� $
��0,10q� �� $	��100 ��� $��* �.+��� $��Bottom temp. [C]��$��Center=��� -   $��LowTempText��$	��100��$��Right=�$��Center=�$��1��� $
��0,10q� �� $	��100 ��� $��* �.+��� $��Top temp. [C]��$��Center=��� -   $��HighTempText��$	��100��$��Right=�$��Center=�$��1��� $
��0,10q� �� $	��100 ��� $��* �.+��� $��Step [C]��$��Center=��� -   $��StepText��$	��100��$��Right=�$��Center=�$��1��� $��1��$��4��$��10q� �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��* �.+��� $��Active filters��$��16(�$
��Bold"�$��Center=�$��Center=�$��0���� -   $��Dectivate filter��$	��130��$��5,8,5,5q�$��Center=�$��3���� -   $��ActiveFiltersListBox��$
��10,5q�$��4���� $��Output��.���  �� $	��200 ��� $��* � �� $��* ��� $
��Auto �.+�� $��0��$��0��$��2�� �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��* �.+��� $��0��$��	Save Data��$��10q�$��16(�$
��Bold"�$��Center=�$��Center=��� -   $��1��$��Download as CSV��$��10q�� $��2�� �� $
��Auto ��� $
��Auto �.+��� $��13(�$��0��$
��Left=�$#��Normalise to max radiance of:��$��	10,5,10,5q�$
��Wrap=��� -   $��NormaliseRadianceTextBox��$��1��$	��150�� �� TextWrapping$
 Wrap=�$
��Left=�$��	10,5,10,5q�� $��1��.+�  OxyPlot.Wpf.PlotView  -   $ ��RadianceTemperatureOxyPlot��  OxyPlot.Wpf.PlotViewBase	   Model	 �� *	PlotModel+�� $��1��$��1��$��10q�$6��0Use right click / scroll on axes to adjust graph��$��Center=�