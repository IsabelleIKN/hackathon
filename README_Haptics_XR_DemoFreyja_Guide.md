# Haptics XR Demo Guide - Freyja & Kraken Support

This guide explains how to work with the Presence Haptics API, specifically for the Haptics-XR-DemoFreyja scene. You'll learn how to set up body parts, create haptic effects, and integrate with various haptic devices including Freyja and Kraken support.

## Table of Contents
1. [Overview](#overview)
2. [Finding the Demo Scene](#finding-the-demo-scene)
3. [Understanding the System Architecture](#understanding-the-system-architecture)
4. [Creating Body Parts with Haptic Receivers](#creating-body-parts-with-haptic-receivers)
5. [Creating Haptic Emitters](#creating-haptic-emitters)
6. [Creating Base Effects](#creating-base-effects)
7. [Associating .haps Effects](#associating-haps-effects)
8. [Designing Haptic Effects with Haptic Composer](#designing-haptic-effects-with-haptic-composer)
9. [Device Implementation Support](#device-implementation-support)
10. [Troubleshooting](#troubleshooting)

## Overview

The Presence Haptics API provides a unified system for creating haptic experiences across multiple devices including:
- **Interhaptics** devices (Quest controllers, etc.)
- **SenseGlove** haptic gloves
- **Actronika** (Skinetic) haptic suits
- **Freyja & Kraken** haptic devices

## Finding the Demo Scene

The Haptics-XR-DemoFreyja scene demonstrates the haptic system in action. Look for demo-related files in:
- `Assets/Presence-Haptics/Core/Demo/`
- Demo UI components and examples are located in this folder

## Understanding the System Architecture

The haptic system consists of several key components:

### Core Components
- **PHap_Core**: Central API that manages all haptic implementations
- **PHap_HapticEffect**: Individual haptic effects with intensity, volume, and repeat settings
- **PHap_BaseEffect**: Base haptic data that can be used across multiple implementations
- **PHap_HapticReceiver**: Defines body parts that can receive haptic feedback
- **PHap_HapticEmitter**: Objects that trigger haptic effects when colliding with receivers

### Device Implementations
- **PHap_InterhapticsImpl**: Supports Interhaptics devices (.haps files)
- **PHap_SenseGloveImpl**: Supports SenseGlove devices (.asset files)
- **PHap_ActronikaImpl**: Supports Actronika/Skinetic devices (.spn files)

## Creating Body Parts with Haptic Receivers

### Step 1: Add a PHap_HapticReceiver Component

1. Create a GameObject representing a body part (e.g., "Chest", "LeftHand")
2. Add a **Collider** component (must be set as **Trigger**)
3. Add the **PHap_HapticReceiver** component

### Step 2: Configure the Receiver

```csharp
public class PHap_HapticReceiver : MonoBehaviour
{
    [SerializeField] protected PHap_BodyPart bodyPart = PHap_BodyPart.Unknown;
    [SerializeField] protected Transform bodyPartOrigin;
    public Vector3 BoundingBoxCenter = Vector3.zero;
    public Vector3 BoundingBoxWidth = new Vector3(1.0f, 1.0f, 1.0f);
}
```

**Configuration Steps:**
1. **Body Part**: Select the appropriate body part from the dropdown:
   - `Torso` (for Actronika/Skinetic)
   - `LeftHandPalm`, `RightHandPalm` (for SenseGlove/Interhaptics)
   - `LeftHead`, `RightHead` (for Interhaptics)
   - `LeftChest`, `RightChest`, `LeftWaist`, `RightWaist` (for Interhaptics)
   - `LeftUpperLeg`, `RightUpperLeg` (for Interhaptics)

2. **Body Part Origin**: Set the Transform that represents the origin/center of this body part

3. **Bounding Box**: Define the spatial boundaries for haptic effects
   - **Center**: Local position offset from the origin
   - **Width**: Dimensions of the haptic area (X, Y, Z)

### Step 3: Events (Optional)
The receiver provides Unity Events for custom haptic handling:
- `OnHapticsEnter`: When an emitter starts touching this receiver
- `OnHapticsStay`: While an emitter is touching this receiver  
- `OnHapticsExit`: When an emitter stops touching this receiver

## Creating Haptic Emitters

### Step 1: Add a PHap_HapticEmitter Component

1. Create a GameObject that will trigger haptic effects (e.g., "Bullet", "Explosion", "TouchObject")
2. Add a **Collider** component (must be set as **Trigger**)
3. Add the **PHap_HapticEmitter** component

### Step 2: Configure the Emitter

```csharp
public class PHap_HapticEmitter : MonoBehaviour
{
    [SerializeField] protected PHap_HapticEffect[] hapticEffects;
    [SerializeField] protected float effectSize = 10.0f;
    [SerializeField] protected bool stopHapticsOnExit = true;
    [SerializeField] protected bool keepUpdatingLocation = false;
}
```

**Configuration Steps:**
1. **Haptic Effects**: Array of PHap_HapticEffect components to play
2. **Effect Size**: World space size of the haptic effect (relevant for spatial haptics)
3. **Stop Haptics On Exit**: Whether to stop effects when collision ends
4. **Keep Updating Location**: Whether to continuously update effect position during collision

### Step 3: Events (Optional)
Similar to receivers, emitters provide Unity Events:
- `OnHapticsEnter`, `OnHapticsStay`, `OnHapticsExit`

## Creating Base Effects

### Step 1: Create a Base Effect Asset

1. **Right-click** in the Project window
2. Go to **Create > Presence > Base Effect**
3. Name your base effect (e.g., "BulletImpact", "Explosion")

### Step 2: Configure the Base Effect

The base effect will be automatically configured when you associate haptic files with it. The system supports:

- **Duration**: Effect length in seconds
- **Effect Type**: Type of haptic feedback (Vibrotactile, Force, Stiffness, etc.)
- **File Links**: Associated device-specific files

## Associating .haps Effects

### Step 1: Prepare Your .haps File

1. Create or obtain a `.haps` file (Interhaptics format)
2. Place it in a folder within your project (preferably in `StreamingAssets` or a `Resources` folder)

### Step 2: Import and Transcode

The system automatically handles transcoding when you:

1. **Drag and drop** the `.haps` file into your project
2. The **PHap_AssetImporter** will automatically:
   - Create a base effect
   - Transcode the file for compatible implementations
   - Generate appropriate ScriptableObject assets

### Step 3: Verify the Import

Check that the base effect now has:
- Correct **Effect Type** (usually `Vibrotactile` for .haps files)
- Proper **Duration** value
- **File Links** pointing to the transcoded files

## Creating Haptic Effects

### Step 1: Add PHap_HapticEffect Component

1. Add the **PHap_HapticEffect** component to a GameObject
2. This can be the same GameObject as your emitter or a separate one

### Step 2: Configure the Haptic Effect

```csharp
public class PHap_HapticEffect : MonoBehaviour
{
    [SerializeField] protected PHap_BaseEffect baseEffect;
    [SerializeField] [Range(0.0f, 5.0f)] protected float intensity = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] protected float volume = 1.0f;
    [SerializeField] protected int repeatAmount = 1;
    [SerializeField] protected bool looping = false;
}
```

**Configuration Steps:**
1. **Base Effect**: Assign your created base effect asset
2. **Intensity**: Base strength of the effect (0.0 - 5.0)
3. **Volume**: Global volume multiplier (0.0 - 1.0) - provides easy volume control
4. **Repeat Amount**: How many times to repeat the effect
5. **Looping**: Whether the effect should loop indefinitely

### Step 3: Link to Emitter

If using an emitter:
1. Add the **PHap_HapticEffect** to the emitter's **Haptic Effects** array
2. Or let the emitter auto-detect effects on the same GameObject

## Designing Haptic Effects with Haptic Composer

### Haptic Composer Overview

The Haptic Composer is Interhaptics' tool for creating custom haptic effects. It allows you to design complex haptic patterns with precise timing and intensity control.

### Getting Started with Haptic Composer

1. **Visit**: [Interhaptics Haptic Composer](https://www.interhaptics.com/tools/haptic-composer)
2. **Create Account**: Sign up for free access to the tool
3. **Web-based Tool**: No installation required, works in your browser

### Creating Your First Effect

1. **New Project**: Start with a blank haptic project
2. **Timeline Editor**: Use the visual timeline to design your effect
3. **Intensity Curves**: Draw amplitude curves for different haptic channels
4. **Frequency Control**: Adjust vibration frequency over time
5. **Preview**: Test your effect in real-time

### Exporting for Unity

1. **Export Format**: Choose `.haps` format for Interhaptics compatibility
2. **Download**: Save the file to your computer
3. **Import to Unity**: Drag the `.haps` file into your Unity project
4. **Auto-import**: The Presence Haptics API will automatically process it


## Device Implementation Support

### Interhaptics (.haps files)
**Supported Body Parts:**
- Head (Left/Right)
- Hand Palm (Left/Right)  
- Chest (Left/Right)
- Waist (Left/Right)
- Upper Leg (Left/Right)

**Supported Effects:**
- Vibrotactile

### SenseGlove (.asset files)
**Supported Body Parts:**
- Hand Palm (Left/Right)
- All fingers (Thumb, Index, Middle, Ring, Pinky)

**Supported Effects:**
- Vibrotactile
- Force Feedback
- Stiffness

### Actronika/Skinetic (.spn files)
**Supported Body Parts:**
- Torso (full body suit)

**Supported Effects:**
- Vibrotactile

## Troubleshooting

### Common Issues

1. **No Haptic Feedback**
   - Check that colliders are set as **Triggers**
   - Verify body parts are supported by your device implementation
   - Ensure the device is connected and detected

2. **Effect Not Loading**
   - Check file path and format compatibility
   - Verify the base effect has proper file links
   - Look for error messages in the Console

3. **Wrong Body Part**
   - Verify the body part is supported by your haptic device
   - Check the PHap_HapticReceiver body part assignment
   - Ensure proper body mapping in your implementation

### Debug Tools

- **Console Logs**: Enable debug output for device implementations
- **Scene Gizmos**: Haptic receivers show bounding boxes when selected
- **Unity Events**: Use receiver/emitter events for debugging collision detection

## Additional Resources

- **Interhaptics Documentation**: [docs.interhaptics.com](https://docs.interhaptics.com)
- **SenseGlove Documentation**: [docs.senseglove.com](https://docs.senseglove.com)
- **Haptic Composer Tool**: [interhaptics.com/tools/haptic-composer](https://www.interhaptics.com/tools/haptic-composer)
- **Unity Haptics Best Practices**: [unity.com/features/xr](https://unity.com/features/xr)

---