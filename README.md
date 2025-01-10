# Hey-Attributes for Unity

A comprehensive collection of custom attributes to enhance Unity Inspector functionality and improve workflow. These attributes provide advanced customization options for displaying and managing properties in the Unity Editor.

## Features

- Rich set of inspector customization attributes
- Serializable dictionary implementation
- Save/Load system for game objects
- Advanced property decorators
- Method buttons and dropdowns
- Component and GameObject references
- Conditional display attributes

## Installation

1. Open the Package Manager in Unity (Window > Package Manager)
2. Click the "+" button in the top-left corner
3. Choose "Add package from git URL..."
4. Enter: `https://github.com/JahnStar/Hey-Attributes.git`

Or simply copy the contents of this repository into your Unity project's Packages folder.

## Attribute Documentation

- Using Hey Attributes
```csharp
using JahnStarGames.Attributes;
```

### HeySerializableDictionary
A serializable dictionary implementation for Unity that allows you to store key-value pairs that persist in the Unity Inspector.

```csharp
[SerializeField] private HeySerializableDictionary<string, int> myDictionary;
```

### HeySave
Save and load system for game objects using JSON serialization. Mark fields to be saved and loaded.

```csharp
[HeySave("playerData")] 
private PlayerStats stats;

// Save data
HeySave.Save("playerData");

// Load all saved data
HeySave.LoadAll();
```
Note: This feature is experimental and in development.

### HeyHelpBox
Display help messages in the inspector with different message types and conditions.

```csharp
[HeyHelpBox("Please assign a manager.", HeyHelpBoxAttribute.BoxType.Error, nameof(manager), conditionValue: null)]
[HeyObjectField(required: true, minimizeWhenNull: true)]
[SerializeField] private ManagerMono manager;
```

### HeyObjectField
Enhanced object field with additional features like required flag and null state visualization.

```csharp
[HeyObjectField(required: true, minimizeWhenNull: true)]
[SerializeField] private GameObject requiredObject;
```

### HeyShowIf
Conditionally show fields based on other property values.

```csharp
[SerializeField] private bool showAdvanced;
[HeyShowIf("showAdvanced", true)]
[SerializeField] private float advancedValue;
```

### HeyReadonly
Make fields read-only in the inspector while keeping them serialized.

```csharp
[HeyReadonly("Current Health")]
[SerializeField] private float health;
```

### HeyMethodButton
Create buttons in the inspector that call specified methods.

```csharp
[HeyMethodButton("Reset Stats", "Reset")]
private void ResetStats() {
    // Reset logic here
}
```

### HeySerializeField
Custom serialization for fields with support for foldouts.

```csharp
[HeySerializeField("mainStats.health", "mainStats.mana")]
[SerializeField] private PlayerStats playerStats;
```

### HeyLabel
Customize field labels with colors and styling.

```csharp
[HeyLabel("Player Speed", LabelColor.Blue, fieldBox: true)]
[SerializeField] private float moveSpeed;
```

### HeyGameObjectName
Reference GameObjects by name with drag-and-drop support.

```csharp
[HeyGameObjectName("Main Camera")]
[SerializeField] private string cameraObjectName;
```

### HeyExpandable
Make ScriptableObject fields expandable in the inspector.

```csharp
[HeyExpandable]
[SerializeField] private ItemData itemData;
```

### HeyComponentDropdown
Dropdown selection for component types.

```csharp
[HeyComponentDropdown]
[SerializeField] private string selectedComponent;
```

### HeyMethodDropdown
Create dropdowns for method selection.

```csharp
[HeyMethodDropdown("targetComponent")]
[SerializeField] private string selectedMethod;
```

Note: Some features are experimental and in development. Please test thoroughly before using in production.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Author

Developed by Halil Emre Yildiz (2023-2024)

## Support

For issues and feature requests, please use the GitHub issue tracker.
