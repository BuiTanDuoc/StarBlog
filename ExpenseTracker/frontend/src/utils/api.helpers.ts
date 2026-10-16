import { getCookie } from "@/utils/cookies";

//Helper function that creates HTTP headers with JWT authorization token for API requests.


// Factory function that creates headers object for API requests
export const getAuthHeaders = () => {
    // Try to get the JWT token from cookies
    const token = getCookie("access_token");

    return {
        "Content-Type": "application/json",  // Tell server we're sending JSON
        // Spread operator with conditional: only add Authorization if token exists
        // This is the "conditional property" pattern
        ...(token && { Authorization: `Bearer ${token}` }),
        // Breakdown:
        // - (token && {...}) evaluates to object if token exists, false if not
        // - ...false spreads nothing, ...{Authorization: ...} spreads the property
    };
};