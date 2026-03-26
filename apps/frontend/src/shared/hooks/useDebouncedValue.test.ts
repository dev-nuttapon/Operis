import { describe, it, expect, vi } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { useDebouncedValue } from "./useDebouncedValue";

describe("useDebouncedValue", () => {
  it("should return initial value immediately", () => {
    const { result } = renderHook(() => useDebouncedValue("initial", 500));
    expect(result.current).toBe("initial");
  });

  it("should debounce value changes", () => {
    vi.useFakeTimers();
    const { result, rerender } = renderHook(
      ({ value, delay }) => useDebouncedValue(value, delay),
      {
        initialProps: { value: "initial", delay: 500 },
      }
    );

    // Update value
    rerender({ value: "updated", delay: 500 });

    // Value should still be 'initial' immediately after update
    expect(result.current).toBe("initial");

    // Fast-forward time by 499ms
    act(() => {
      vi.advanceTimersByTime(499);
    });
    expect(result.current).toBe("initial");

    // Fast-forward by 1ms (total 500ms)
    act(() => {
      vi.advanceTimersByTime(1);
    });
    expect(result.current).toBe("updated");

    vi.useRealTimers();
  });

  it("should clear previous timeout when value changes rapidly", () => {
    vi.useFakeTimers();
    const { result, rerender } = renderHook(
      ({ value, delay }) => useDebouncedValue(value, delay),
      {
        initialProps: { value: "initial", delay: 500 },
      }
    );

    // Update value to 'v1'
    rerender({ value: "v1", delay: 500 });

    // Advance 250ms
    act(() => {
      vi.advanceTimersByTime(250);
    });

    // Update value to 'v2' before 'v1' is set
    rerender({ value: "v2", delay: 500 });

    // Advance another 300ms (total 550ms since v1 update, but only 300ms since v2 update)
    act(() => {
      vi.advanceTimersByTime(300);
    });

    // Should still be 'initial' because v2 delay hasn't finished
    expect(result.current).toBe("initial");

    // Advance another 200ms (total 500ms since v2 update)
    act(() => {
      vi.advanceTimersByTime(200);
    });

    expect(result.current).toBe("v2");
    vi.useRealTimers();
  });
});
