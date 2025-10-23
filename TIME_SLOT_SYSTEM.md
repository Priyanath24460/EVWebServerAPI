# EV Charging Station Time Slot Booking System

## 🎯 **System Overview**

Your EV charging station booking system now supports **hourly time slots** with the following features:

### ✅ **Key Features**
- **3 charging points** per station
- **24 hourly time slots** per day (00:00-01:00, 01:00-02:00, etc.)
- **Red/Green availability display** (booked vs available)
- **Real-time conflict prevention**
- **Auto-approval for valid bookings**

## 🏗️ **Database Structure**

### Updated Booking Model
```csharp
public class Booking
{
    public string Id { get; set; }
    public string BookingReference { get; set; }
    public string EVOwnerNIC { get; set; }
    public string ChargingStationId { get; set; }
    
    // NEW TIME SLOT FIELDS
    public int ChargingPointNumber { get; set; }  // 1, 2, or 3
    public DateTime BookingDate { get; set; }     // Date only (YYYY-MM-DD)
    public int TimeSlot { get; set; }             // 0-23 (hour of day)
    public DateTime StartTime { get; set; }       // Full start datetime
    public DateTime EndTime { get; set; }         // Full end datetime
    
    public int DurationMinutes { get; set; } = 60;
    public string Status { get; set; } = "Approved";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

## 🔌 **API Endpoints**

### 1. Get Station Availability (All Time Slots)
```http
GET /api/timeslots/station/{stationId}/availability?date=2025-10-24
```
**Response:** Complete grid showing all 72 time slots (3 points × 24 hours) with booking status

### 2. Get Available Time Slots Only
```http
GET /api/timeslots/station/{stationId}/available?date=2025-10-24
```
**Response:** Only free time slots (filters out booked ones)

### 3. Check Specific Time Slot
```http
GET /api/timeslots/station/{stationId}/point/{pointNumber}/check?date=2025-10-24&timeSlot=9
```
**Response:** `{"isAvailable": true/false}`

### 4. Create Time Slot Booking
```http
POST /api/timeslots/book
Content-Type: application/json

{
  "chargingStationId": "station_id_here",
  "evOwnerNIC": "123456789V",
  "chargingPointNumber": 1,        // 1, 2, or 3
  "bookingDate": "2025-10-24",
  "timeSlot": 9                    // 0-23 (9 = 09:00-10:00)
}
```

## 📱 **Mobile App Integration**

### User Booking Flow:
1. **Select Station** → User picks charging station from map
2. **Choose Date** → User selects booking date
3. **View Time Grid** → App calls `/availability` endpoint
4. **Display Grid:**
   ```
   Point 1: [🟢][🔴][🟢][🔴][🟢]... (24 slots)
   Point 2: [🟢][🟢][🔴][🟢][🟢]...
   Point 3: [🔴][🟢][🟢][🟢][🔴]...
   ```
5. **User Selects** → Tap green slot
6. **Book Slot** → App calls `/book` endpoint
7. **Success** → Slot turns red for other users

### Response Format Example:
```json
{
  "stationId": "672e52c7b97e47b5c3fcee11",
  "stationName": "Colombo Main Station",
  "date": "2025-10-24",
  "chargingPoints": [
    {
      "chargingPointNumber": 1,
      "timeSlots": [
        {
          "hour": 0,
          "timeRange": "00:00 - 01:00",
          "isAvailable": true,
          "bookedBy": null,
          "bookingId": null
        },
        {
          "hour": 9,
          "timeRange": "09:00 - 10:00",
          "isAvailable": false,
          "bookedBy": "123456789V",
          "bookingId": "booking_id_here"
        }
      ]
    }
  ]
}
```

## 🚫 **Conflict Prevention**

The system prevents double-booking through:

1. **Database Validation** → Checks existing bookings before creating new ones
2. **Atomic Operations** → Uses MongoDB transactions where possible
3. **Real-time Updates** → Other users see booked slots immediately

### Booking Conflicts:
- ❌ Same charging point + same time slot + same date
- ✅ Different charging point + same time slot + same date
- ✅ Same charging point + different time slot + same date

## 🎨 **UI Color Coding**

For your mobile app display:
- 🟢 **Green** = Available time slot
- 🔴 **Red** = Booked time slot
- ⚪ **Gray** = Past time slots (optional)

## 📋 **Example Usage**

1. **Get availability for today:**
   ```
   GET /api/timeslots/station/672e52c7b97e47b5c3fcee11/availability?date=2025-10-24
   ```

2. **Book 2 PM slot on charging point 2:**
   ```json
   POST /api/timeslots/book
   {
     "chargingStationId": "672e52c7b97e47b5c3fcee11",
     "evOwnerNIC": "123456789V",
     "chargingPointNumber": 2,
     "bookingDate": "2025-10-24",
     "timeSlot": 14
   }
   ```

## 🛠️ **Testing**

Use the test file: `TimeSlotTests.http` to test all endpoints with sample data.

Your time slot booking system is now fully implemented and ready for mobile app integration! 🚀