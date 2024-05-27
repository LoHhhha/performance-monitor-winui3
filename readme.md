# 效率工具箱文档

效率工具箱是由`LoHhhha`开发的一款基于`Windows SDK 1.5`开发的软件。旨在实现一些极客功能如：计算机性能监测、网络IP地址查看、TODO列表、日程表等功能，同时集成`Dell G15`系列`AWCC-WMI`相关接口，实现风扇转速调整、性能模式切换的功能。

注：文档给出的前端设置均为简化版，未指定任何参数，仅做参考，具体请参见对应源代码。

# 模块

## 性能监视器模块

### 性能页面`Monitor`

#### 实现的功能

* 实时显示设备工作情况
* 显示的项目完全自定义

#### 依赖工具

* `Performance`目录

#### 前端设置

```xaml
<ScrollView>
    <Grid x:Name="DisplayGrid">
    </Grid>
</ScrollView>
```

#### 实现方法

* 全部元素动态生成，使用`performance_monitor_winui3.Tools.Utils`类中的工具完成对控件的动态生成，添加到`DisplayGrid`。
    * 为什么要动态生成？由于本页中的所有子模块都可以随意组合、随意移动位置，故本页所有显示控件都是动态生成的。
    * 方法包括使用到的方法有`AddTitle2Grid`、`AddPairGrid2Grid`、`AddPairWithProgressBarGrid2Grid`、`AddLoadingProgressBar2Grid`，用于生成对应的子项。
    * 使用文字-数值对、文字-进度条-文字对、文字标题表示各子项。
* 使用`Information.TotalInformation`完成对设备信息的收集。
* 在对应`ViewModel`中使用`_selectedTypes`来确定显示的项目。
* 使用`Microsoft.UI.Xaml.DispatcherTimer`的`timer`来在每一个需要更新的时刻更新`DisplayGrid`。
* 使用`System.Threading.Semaphore`的`timerNotRunning`来确保`timer`产生的任务只有一个能对`Information.TotalInformation`进行操控。
* 使用异步优化界面，尽量不阻塞UI线程。
* 当从页面退出时，也就是调用`OnNavigatedFrom`时停止`timer`。
* 当回到页面时，也就是调用`OnNavigatedTo`时继续`timer`。

#### 页面展示

<center class="half">
    <img src=".\picture\monitor_page_0.png" width="300"/>
    <img src=".\picture\monitor_page_1.png" width="300"/>
</center>

* 多种性能指标随意组合，完全自定义
* 刷新时间间隔自由定义

### 网络页面`Network`

#### 实现的功能

* 显示各网卡工作情况，包括IP地址、DHCP服务器等

#### 依赖工具

* `Performance`目录

#### 前端设置

```xaml
<ScrollView>
    <Grid x:Name="DisplayGrid">
    </Grid>
</ScrollView>
```

#### 实现方法

* 全部元素动态生成，使用`performance_monitor_winui3.Tools.Utils`类中的工具完成对控件的动态生成，添加到`DisplayGrid`。
* 使用`Microsoft.UI.Xaml.DispatcherTimer`的`timer`来在每一个需要更新的时刻更新`DisplayGrid`。
* 使用`System.Threading.Semaphore`的`timerNotRunning`来确保`timer`产生的任务只有一个能对`Information.TotalInformation`进行操控。
* 使用异步优化界面，尽量不阻塞UI线程。
* 当从页面退出时，也就是调用`OnNavigatedFrom`时停止`timer`。
* 当回到页面时，也就是调用`OnNavigatedTo`时继续`timer`。

#### 页面展示

<center class="half">
    <img src=".\picture\network_page_0.png" width="300"/>
    <img src=".\picture\network_page_1.png" width="300"/>
</center>


### 超载页面`Overclock`

#### 实现的功能

* 实时显示设备`CPU`、`GPU`温度，`CPU`、`GPU`对应风扇温度
* 实现风扇转速调节、性能模式切换。
* 注：仅`Dell G15`系列可用。

#### 依赖工具

* `AWCC`目录

#### 前端设置

```xaml
<Grid>
    <ScrollView>
        <StackPanel>
            <InfoBar Name="ErrorInfoBar"/>
            <Grid>
                <TextBlock/>
                <tc:RadialGauge/>
                <tc:RadialGauge/>
                <TextBlock/>
                <TextBlock/>
            </Grid>

            <Grid>
                <TextBlock/>
                <tc:RadialGauge/>
                <tc:RadialGauge/>
                <TextBlock/>
                <TextBlock/>
            </Grid>


            <Grid>
                <TextBlock/>
                <ComboBox>
                    <ComboBoxItem/>
                    <ComboBoxItem/>
                    <ComboBoxItem/>
                </ComboBox>
            </Grid>

            <Grid Name="SliderGrid" Visibility="Collapsed">
                <TextBlock/>
                <TextBlock/>
                <Slider/>
                <TextBlock/>
                <Slider/>
            </Grid>
        </StackPanel>
    </ScrollView>
</Grid>
```

#### 实现方法

* 首先调用`AWCCWMI`类方法，确认是否存在`AWCCWmiMethodFunction`的WMI服务，如果不存在，将错误信息打印在`ErrorInfoBar`。
* 从`AWCCWMI`类方法获取具体的信息，更新到对应的控件。
* 使用`Microsoft.UI.Xaml.DispatcherTimer`的`timer`来在每一个需要更新的时刻更新`DisplayGrid`。
* 使用`System.Threading.Semaphore`的`timerNotRunning`来确保`timer`产生的任务只有一个能对`Information.TotalInformation`进行操控。
* 使用异步优化界面，尽量不阻塞UI线程。
* 当从页面退出时，也就是调用`OnNavigatedFrom`时停止`timer`。
* 当回到页面时，也就是调用`OnNavigatedTo`时继续`timer`。

#### 页面展示

<center class="half">
    <img src=".\picture\overclock_page_0.png" width="300"/>
    <img src=".\picture\overclock_page_1.png" width="300"/>
</center>

* 在选择用户模式时会将滑动条显示并允许调节。

## 实用工具模块

### 待办页面`ToDoList`

#### 实现的功能

* 待办事项的查看、增加、删除、修改、改变位置、完成标记。
* 已办事项的查看、删除。

#### 前端设置

```xaml
<Grid>
    <ScrollView>
        <StackPanel>
            <TextBlock>
                <Run/>
            </TextBlock>
            <ListView ItemsSource="{x:Bind ViewModel.ToDoListItems}">
                <ListView.ItemTemplate>
                    <DataTemplate x:DataType="models:ToDoListItem">
                        <Border>
                            <Border.ContextFlyout>
                                <MenuFlyout>
                                    <MenuFlyoutItem/>
                                    <MenuFlyoutItem/>
                                    <MenuFlyoutSeparator/>
                                    <MenuFlyoutItem/>
                                    <MenuFlyoutItem/>
                                    <MenuFlyoutItem/>
                            </Border.ContextFlyout>
                            <Grid>
                                <TextBlock/>
                                <TextBlock/>
                                <TextBlock/>
                            </Grid>
                        </Border>
                    </DataTemplate>
                </ListView.ItemTemplate>
            </ListView>
            <TextBlock>
                <Run/>
            </TextBlock>
            <ListView ItemsSource="{x:Bind ViewModel.FinishListItems}">
                <ListView.ItemTemplate>
                    <DataTemplate x:DataType="models:FinishListItem">
                        <Border>
                            <Border.ContextFlyout>
                                <MenuFlyout>
                                    <MenuFlyoutItem/>
                                    <MenuFlyoutItem/>
                                    <MenuFlyoutSeparator/>
                                    <MenuFlyoutItem/>
                                </MenuFlyout>
                            </Border.ContextFlyout>
                            <Grid Margin="10">
                                <TextBlock/>
                                <TextBlock/>
                                <TextBlock/>
                                <TextBlock/>
                                <TextBlock/>
                            </Grid>
                        </Border>
                    </DataTemplate>
                </ListView.ItemTemplate>
            </ListView>
        </StackPanel>
    </ScrollView>
    <Button/>
</Grid>
```

#### 实现方法

* `MVVM`模式开发。
  * 持久化封装为`ToDoListDao`类，负责控制与嵌入式数据库`LiteDB`交互。
  * `ObservableCollection<T>`类型的`ToDoListItems`、`FinishListItems`存在于`ViewModel`当中，在对其操作的同时，使用`ToDoListDao`同步完成持久化（数据量不大，所以使用同步操作）。
  * 前端使用绑定模式显示。
* 针对每个行为绑定一个委托，完成对列表元素的操作。
* `ToDoListItems`用于记录添加过的待办。
* `FinishListItems`用于记录已经完成的事情。
* 使用动态生成的`ContentDialog`完成对操作的可视化。

#### 页面展示

* 单击对应的项目可以查看详细项目情况，光标滑到对应项目项目边框加粗。
<center class="half">
    <img src=".\picture\todo_page_0.png" width="300"/>
    <img src=".\picture\todo_page_1.png" width="300"/>
</center>

* 右击项目显示更多对项目的操作。
<center class="half">
    <img src=".\picture\todo_page_2.png" width="300"/>
    <img src=".\picture\todo_page_5.png" width="300"/>
</center>

* 添加、修改操作有对应的值校验。
<center class="half">
    <img src=".\picture\todo_page_3.png" width="300"/>
    <img src=".\picture\todo_page_4.png" width="300"/>
</center>

### 事务页面`Access`

#### 实现的功能

* 事务自定义
  * 指定事务名称
  * 指定事务执行的`CMD`命令，可多行
  * 指定执行时是否保留窗口显示
  * 指定执行完毕后是否关闭窗口
* 事务的查看、增加、删除、修改、改变位置、执行。

#### 前端设置

```xaml
<Grid>
	<ScrollView>
		<StackPanel>
			<ListView ItemsSource="{x:Bind ViewModel.ErrorMessages}">
				<ListView.ItemTemplate>
					<DataTemplate x:DataType="models:AccessRunError">
						<InfoBar/>
					</DataTemplate>
				</ListView.ItemTemplate>
			</ListView>
			
			<ListView ItemsSource="{x:Bind ViewModel.AccessItems}">
				<ListView.ItemTemplate>
					<DataTemplate x:DataType="models:AccessItem">
						<Border>
							<Border.ContextFlyout>
								<MenuFlyout>
                                    <MenuFlyoutItem/>
                                    <MenuFlyoutItem/>
                                    <MenuFlyoutSeparator/>
                                    <MenuFlyoutItem/>
                                    <MenuFlyoutItem/>
                                    <MenuFlyoutItem/>
								</MenuFlyout>
							</Border.ContextFlyout>
							<Grid>
								<TextBlock/>
                                <Button/>
							</Grid>
						</Border>
					</DataTemplate>
				</ListView.ItemTemplate>
			</ListView>
		</StackPanel>
	</ScrollView>
	<Button/>
</Grid>
```

#### 实现方法

* `MVVM`模式开发。
  * 持久化封装为`AccessDao`类，负责控制与嵌入式数据库`LiteDB`交互。
  * `ObservableCollection<T>`类型的`ErrorMessages`、`AccessItems`存在于`ViewModel`当中，在对其操作的同时，使用`AccessDao`同步完成持久化（数据量不大，所以使用同步操作，`ErrorMessages`不做持久化）。
  * 前端使用绑定模式显示。
* `ErrorMessages`用于记录之前执行的任务出现的错误。
* `AccessItems`用于记录添加过的事务。
* 使用动态生成的`ContentDialog`完成对操作的可视化。

#### 页面展示

* 光标滑到对应项目项目边框加粗。
<center class="half">
    <img src=".\picture\access_page_0.png" width="300"/>
    <img src=".\picture\access_page_2.png" width="300"/>
</center>

* 单击对应的项目可以查看详细项目情况，同样可以右键显示更多，此处不展示。添加时对多选项进行了约束如果不显示窗口则一定要完成后退出。
<center class="half">
    <img src=".\picture\access_page_1.png" width="300"/>
    <img src=".\picture\access_page_3.png" width="300"/>
</center>

### 日程页面`Schedule`

#### 实现的功能

* 日程的添加
  * 可选择星期或是每天需要完成
  * 日程的名称
  * 日程的备注
  * 需要完成的时间段
* 日程自动按开始时间排序。
* 在顶部可选择感兴趣的星期需要，下方列表显示对应的工作。
* 首次打开默认显示当天对应的星期选项。

#### 前端设置

```xaml
<Grid>
	<tc:Segmented>
		<tc:SegmentedItem Content="Mon"/>
		<tc:SegmentedItem Content="Tue"/>
		<tc:SegmentedItem Content="Wed"/>
		<tc:SegmentedItem Content="Thu"/>
		<tc:SegmentedItem Content="Fri"/>
        <tc:SegmentedItem Content="Sat"/>
        <tc:SegmentedItem Content="Sun"/>
    </tc:Segmented>
	<ScrollView>
        <StackPanel>
            <ProgressRing/>
			<ListView ItemsSource="{x:Bind ViewItems}">
				<ListView.ItemTemplate>
					<DataTemplate x:DataType="models:ScheduleItem">
						<Border>
							<Border.ContextFlyout>
								<MenuFlyout>
									<MenuFlyoutItem/>
									<MenuFlyoutSeparator/>
									<MenuFlyoutItem/>
									<MenuFlyoutItem/>
								</MenuFlyout>
							</Border.ContextFlyout>
							<Grid>
								<TextBlock/>
								<TextBlock/>
                                <TextBlock/>
								<TextBlock/>
							</Grid>
						</Border>
					</DataTemplate>
				</ListView.ItemTemplate>
			</ListView>
        </StackPanel>
    </ScrollView>
	<Button/>
</Grid>
```

#### 实现方法

* `MVVM`模式开发。
  * 持久化封装为`ScheduleDao`类，负责控制与嵌入式数据库`LiteDB`交互。
  * `ObservableCollection<T>`类型的`ViewItems`，在对其操作的同时，使用`ScheduleDao`同步完成持久化。
  * 前端使用绑定模式显示。
* `ViewItems`用于记录当前选择下应该显示的日程。
* 使用动态生成的`ContentDialog`完成对操作的可视化。

#### 页面展示

* 根据当前时间自动选择对应的星期，并显示对应的日程。
<center class="half">
    <img src=".\picture\schedule_page_0.png" width="300"/>
    <img src=".\picture\schedule_page_1.png" width="300"/>
</center>

* 添加值校验、日程可选择每天。
<center class="half">
    <img src=".\picture\schedule_page_2.png" width="300"/>
    <img src=".\picture\schedule_page_3.png" width="300"/>
</center>

### 设置页面`Setting`

#### 实现的功能

* 个性化设置
  * 显示模式：深色、浅色、跟随系统
  * 性能页面`Monitor`展示的元素
  * 性能页面`Monitor`、网络页面`Network`刷新的时间间隔。
* 全球化
  * 语言设置
    * CN-ZH
    * EN-US

#### 前端设置

```xaml
<ScrollView VerticalScrollBarVisibility="Hidden">
    <StackPanel>
        <TextBlock/>
        <StackPanel>
            <!--ThemeSetting-->
            <controls:SettingsCard>
                <ComboBox>
                    <ComboBoxItem/>
                    <ComboBoxItem/>
                    <ComboBoxItem/>
                </ComboBox>
            </controls:SettingsCard>
            <!--ThemeSetting-->

            <!--MonitorOrderSetting-->
            <controls:SettingsExpander>
                <controls:SettingsExpander.ItemsHeader>
                    <ListView ItemsSource="{x:Bind ViewModel.MonitorItems}">
                        <ListView.ItemTemplate>
                            <DataTemplate x:DataType="models:MonitorOrderListItem">
                                <TextBlock/>
                                <StackPanel/>
                                    <Button/>
                                    <Button/>
                                    <Button/>
                                </StackPanel>
                                </Grid>
                            </DataTemplate>
                        </ListView.ItemTemplate>
                    </ListView>
                </controls:SettingsExpander.ItemsHeader>
                <controls:SettingsExpander.ItemsFooter>
                    <Grid>
                        <Button/>
                        <Button/>
                        <Button/>
                    </Grid>
                </controls:SettingsExpander.ItemsFooter>
            </controls:SettingsExpander>
            <!--MonitorOrderSetting-->

            <!--TimerSetting-->
            <controls:SettingsExpander>
                <controls:SettingsExpander.Items>
                    <controls:SettingsCard>
                        <StackPanel>
                            <NumberBox/>
                            <TextBlock/>
                        </StackPanel>
                    </controls:SettingsCard>
                    <controls:SettingsCard>
                        <StackPanel>
                            <NumberBox/>
                            <TextBlock/>
                        </StackPanel>
                    </controls:SettingsCard>
                </controls:SettingsExpander.Items>
            </controls:SettingsExpander>
            <!--TimerSetting-->
        </StackPanel>

        <TextBlock/>
        <controls:SettingsCard>
            <ComboBox>
                <x:String>en-us</x:String>
                <x:String>zh-cn</x:String>
            </ComboBox>
        </controls:SettingsCard>

        <TextBlock/>
        <controls:SettingsExpander>
            <controls:SettingsExpander.Items>
                <controls:SettingsCard>
                    <HyperlinkButton/>
                </controls:SettingsCard>
            </controls:SettingsExpander.Items>
        </controls:SettingsExpander>
    </StackPanel>
</ScrollView>
```

#### 实现方法

* 通过`LiteDB`与`indows.Storage.ApplicationData.Current.LocalSettings`实现持久化。
* 通过修改对应页面的`ViewModel`中的`static`元素或者直接修改持久化位置的信息完成页面间信息交换。
* `Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride`获取、修改当前的全局语言。

#### 页面展示

* 可对性能页面显示项目进行自由的修改、包括显示的元素位置、元素种类。
<center class="half">
    <img src=".\picture\settings_page_0.png" width="300"/>
    <img src=".\picture\settings_page_1.png" width="300"/>
</center>

* 更多设置，包括全球化等更多应用显示设置。
<center class="half">
    <img src=".\picture\settings_page_2.png" width="300"/>
    <img src=".\picture\settings_page_3.png" width="300"/>
</center>

# 自实现类

## `Performance`工具目录

### `LHMSupport`命名空间

#### `LHMReader`工具类

这个类用于操控[`LibreHardwareMonitor.Hardware`](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)，使用类的封装方法，使用前需要对其进行实例化。

* 重要方法
  * `void Update() {}`更新对象所维护的设备信息，在使用对象拥有的设备信息时，需要首先调用该方法，等待该方法完成后才可对其属性进行访问。
* 重要属性
  * `List<CpuInformation> CPU`  处理器信息
  * `List<GpuInformation> GPU`  显卡信息，不处理`Nvidia`显卡
  * `NetworkSpeedInformation Network`   网络流量信息
  * `List<BatteryInformation> Battery`  电池信息
  * `List<DiskInformation> Disk`    硬盘信息
  * `List<MemoryInformation> Memory`    内存信息

* 细节
  * 该工具类存在大量“魔法数字”与硬编码，由于[`LibreHardwareMonitor.Hardware`](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)的特性，暂时无法避免，同时各类计算机平台对应的硬编码也不一致，故此处存在不确定性。
  * `List<T>`提前给定一个较大的值以防不断申请空间，每次执行`List.Clear`。
  * 注意：不要修改重要属性当中的任何值！

### `NvmlSupport`命名空间

#### `NvidiaApi`工具类

这个类是根据[官方文档](https://docs.nvidia.com/deploy/nvml-api/)的对一些关键接口实现使用C#语言调用，同时将一些用到的数据类型在此命名空间完成转换。

#### `NvidiaSmi`工具类

这个类用于操控`Nvml Tool`，使用单例模式，使用时需要使用`GetInstance`获取实例，而不是通过实例化获取实例。

* 重要方法
  * `void Update() {}`更新对象所维护的设备信息，在使用对象拥有的设备信息时，需要首先调用该方法，等待该方法完成后才可对其属性进行访问。
* 重要属性
  * `List<GpuInformation> GPU`  `Nvidia`显卡信息
  * `bool NvidiaSmiIsAvailable = false`

* 细节
  * 如果不存在对应的`NvidiaApi`，本类调用将不会发生任何作用。
  * `List<T>`提前给定一个较大的值以防不断申请空间，每次执行`List.Clear`。
  * 注意：不要修改重要属性当中的任何值！

### `HardwareInfoSupport`命名空间

#### `HardwareInfoReader`工具类

这个类用于操控[`Hardware.Info`](https://github.com/Jinjinov/Hardware.Info)，使用类的封装方法，使用前需要对其进行实例化。

* 重要方法
  * `void Update() {}`更新对象所维护的设备信息，在使用对象拥有的设备信息时，需要首先调用该方法，等待该方法完成后才可对其属性进行访问。
* 重要属性
  * `List<NetworkInformation> Network`  网络设备工作情况

* 细节
  * 如果`IHardwareInfo`不可用，本类调用将不会发生任何作用。
  * `List<T>`提前给定一个较大的值以防不断申请空间，每次执行`List.Clear`。
  * 注意：不要修改重要属性当中的任何值！

### `Information`命名空间

#### `TotalInformation`工具类

这个类是用于数据整合，对收集到的信息进行聚合分析，同时添加软件工作情况。该类为静态类，使用方法时请使用其静态方法。

* 重要方法
  * `void Update() {}`更新对象所维护的设备信息（`Monitor`页面使用），在使用对象拥有的设备信息时，需要首先调用该方法，等待该方法完成后才可对其属性进行访问。
  * `void UpdateNetwork() {}`更新对象所维护的设备信息（`Network`页面使用），在使用对象拥有的设备信息时，需要首先调用该方法，等待该方法完成后才可对其属性进行访问。
* 重要属性
  * `List<CpuInformation> CPU`  处理器信息，实际上来源于`LibreReader`
  * `List<GpuInformation> GPU`  所有显卡信息，聚合自`OtherGPU`与`NvidiaGPU`
  * `List<GpuInformation> OtherGPU` 显卡信息，实际上来源于`LibreReader`
  * `List<GpuInformation> NvidiaGPU`    英伟达显卡信息，实际上来源于`NvidiaSmi`
  * `NetworkSpeedInformation NetworkSpeed`  网络流量信息，实际上来源于`LibreReader`
  * `List<NetworkInformation> Network`  网络设备信息，实际上来源于`HardwareInfoReader`
  * `List<BatteryInformation> Battery`  电池信息，实际上来源于`LibreReader`
  * `List<DiskInformation> Disk`    硬盘信息，实际上来源于`LibreReader`
  * `List<MemoryInformation> Memory`    内存信息，实际上来源于`LibreReader`
  * `List<KeyValuePair<string, string>> Outline`    时间等外围信息
  * `List<KeyValuePair<string, string>> RunTimeInfo`    程序运行信息

* 细节
  * `List<T>`提前给定一个较大的值以防不断申请空间，每次执行`List.Clear`。
  * 注意：不要修改重要属性当中的任何值！

## `AWCC`工作目录

### `AWCC`命名空间

#### `AWCCWMI`类

这个类是用于管理`Dell G15`系列的`AWCCWmiMethodFunction`，基于`WMI`接口，获取、控制该系列的传感器信息、风扇转速。基于项目[tcc-g15](https://github.com/AlexIII/tcc-g15)，[wmie2](https://github.com/vinaypamnani/wmie2/)设计得到，再次感谢[tcc-g15](https://github.com/AlexIII/tcc-g15)提供的接口信息。类采用静态类设计，在第一次使用类的时候会自动查找到对应的句柄。

* 重要方法
  * `bool IsAvailable() {}` `AWCC`是否就绪
  * `uint GetFanRPM(uint fanId) {}` 获取给定的`FanId`的转速。
  * `uint GetFanRPMPercent(uint fanId) {}`  获取给定的`FanId`的转速百分比，此方法无效。
  * `uint GetSensorTemperature(uint sensorId) {}`   获取给定的`sensorId`的传感器的值，单位为摄氏度。
  * `bool ApplyThermalMode(ThermalMode mode) {}`    根据给定的`ThermalMode.mode`设置工作模式。
  * `bool SetAddonSpeedPercent(uint fanId, uint addonPercent) {}`   设置给定的`FanId`的转速百分比`addonPercent`(0x00-0xff)。
* 重要属性
  * `string ErrorMsg`   若发生错误则将错误原因填入，默认值为""
  * `uint CpuFanId` 处理器风扇Id
  * `uint GpuFanId` 显卡风扇Id
  * `uint CpuSensorId`  处理器温度传感器Id
  * `uint GpuSensorId`  显卡温度传感器Id
  * `ThermalMode Mode`  目前工作状态
* 细节
  * 每次启动会将系统强制工作在平衡模式`ThermalMode.Balanced`，这是因为不存在模式查询的接口。
  * 注意：不要修改重要属性当中的任何值！

## `Tools`工作目录

### `StringResource`类

